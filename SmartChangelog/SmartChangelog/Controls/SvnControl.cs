using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

using SharpSvn;
using SharpSvn.Remote;
using SharpSvn.Security;

using SmartChangelog.Definitions;
using SmartChangelog.Tools;

using AForge.Neuro;
using AForge.Neuro.Learning;

namespace SmartChangelog.Controls
{
    class SvnControl
    {
        // 常量定义
        private const string SVN_ADMIN_USERNAME = "jenkins_admin";
        private const string SVN_ADMIN_PASSWORD = "jenkins@@)!&0904";
        private static string ChangeTypeNeuro = Environment.CurrentDirectory
            + System.IO.Path.DirectorySeparatorChar + "neuro"
            + System.IO.Path.DirectorySeparatorChar + "changetype.network";

        private ActivationNetwork mChangeTypeNetwork = null;

        private MainWindow mWin = null;

        public SvnControl(MainWindow win)
        {
            mWin = win;

            try
            {
                mChangeTypeNetwork = Network.Load(ChangeTypeNeuro) as ActivationNetwork;
            }
            catch (Exception)
            {
                // 加载失败, 一般是文件不存在, 此时重置network
                mChangeTypeNetwork = null;
            }
        }

        public List<string> GetSvnBranchList(MainWindow win,
            string serverUri)
        {
            if (string.IsNullOrWhiteSpace(serverUri))
            {
                return null;
            }

            SvnRemoteSession remoteSession = null;
            try
            {
                List<string> branchList = new List<string>();
                Uri uri = new Uri(serverUri);
                remoteSession = new SvnRemoteSession();

                remoteSession.Authentication.Clear();
                remoteSession.Authentication.UserNamePasswordHandlers +=
                    new EventHandler<SvnUserNamePasswordEventArgs>(
                        delegate(object s, SvnUserNamePasswordEventArgs args)
                        {
                            args.UserName = SVN_ADMIN_USERNAME;
                            args.Password = SVN_ADMIN_PASSWORD;
                        });

                bool connect = remoteSession.Open(uri);
                if (connect)
                {
                    remoteSession.List("",
                        new EventHandler<SvnRemoteListEventArgs>(
                            delegate(object s, SvnRemoteListEventArgs args)
                            {
                                branchList.Add(args.Name);
                            }));
                }
                return branchList;
            }
            catch (Exception e)
            {
                if (null != win)
                {
                    win.ShowErrorMessage(win.FindResource("get_svn_branch_list_failed")
                        + Environment.NewLine
                        + win.FindResource("reason") + ": " + e.Message);
                }
                return null;
            }
            finally
            {
                if (null != remoteSession)
                {
                    remoteSession.Dispose();
                    remoteSession = null;
                }
            }
        }

        public List<LogItem> GetSvnLogList(string serverUri, string branchName, 
            long startVersion, long endVersion, out bool success, out string err) 
        {
            SvnRemoteSession remoteSession = null;
            try
            {
                List<LogItem> logList = new List<LogItem>();
                Uri uri = new Uri(serverUri);
                remoteSession = new SvnRemoteSession();

                remoteSession.Authentication.Clear();
                remoteSession.Authentication.UserNamePasswordHandlers +=
                    new EventHandler<SvnUserNamePasswordEventArgs>(
                        delegate(object s, SvnUserNamePasswordEventArgs args)
                        {
                            args.UserName = SVN_ADMIN_USERNAME;
                            args.Password = SVN_ADMIN_PASSWORD;
                        });

                bool connect = remoteSession.Open(uri);
                if (connect)
                {
                    remoteSession.Log(branchName, new SvnRevisionRange(startVersion, endVersion),
                        new EventHandler<SvnRemoteLogEventArgs>(
                            delegate(object s, SvnRemoteLogEventArgs args)
                            {
                                LogItem item = new LogItem();
                                item.type = Constant.LogType.Svn;
                                item.author = args.Author;
                                item.revision = args.Revision.ToString();
                                item.content = args.LogMessage;
                                item.time = args.Time;
                                logList.Add(item);
                            }));
                }

                success = true;
                err = null;
                return logList;
            }
            catch (Exception e)
            {
                success = false;
                err = mWin.FindResource("get_svn_log_list_failed") 
                    + Environment.NewLine
                    + mWin.FindResource("reason") + ": " + e.Message;
                return null;
            }
            finally
            {
                if (null != remoteSession)
                {
                    remoteSession.Dispose();
                    remoteSession = null;
                }
            }
        }

        public Changelog AnalyzeLog(List<LogItem> logList, out bool success, out string err)
        {
            Changelog svnChangelog = null;
            try
            {
                svnChangelog = new Changelog();
                foreach (LogItem log in logList)
                {
                    List<ChangeItem> changeList = Parse(log);
                    if (null != changeList && 0 < changeList.Count)
                    {
                        InsertChangelog(svnChangelog, changeList);
                    }
                }
            }
            catch (Exception e)
            {
                success = false;
                err = mWin.FindResource("analyze_svn_log_list_failed")
                    + Environment.NewLine
                    + mWin.FindResource("reason") + ": " + e.Message;
                return null;
            }
            

            success = true;
            err = null;
            return svnChangelog;
        }

        public void LearnData(Changelog changelog, out LearnStatistics statistics)
        {
            statistics = LoadLastStatistics();

            long learnCnt = 0;
            double accuracy = 0.0;
            try
            {
                LearnChangeType(changelog, out learnCnt, out accuracy);
                statistics.StatisticsChangeTypeCnt += learnCnt;
                statistics.StatisticsChangeTypeAccuracy = accuracy;
                SaveCurrentStatistics(statistics);
            }
            catch (Exception e)
            {
                // Ingore
            }
        }

        private List<ChangeItem> Parse(LogItem log) 
        {
            // 一个Log可能会对应多个Change
            List<ChangeItem> changeList = new List<ChangeItem>();

            // 获取事件Id
            List<string> eventIdList = ParseEventId(log);
            foreach (string eventId in eventIdList)
            {
                ChangeItem change = new ChangeItem();
                change.author = log.author;
                change.revision = log.revision;
                change.eventId = eventId;
                change.log = log;
                changeList.Add(change);
            }

            // 获取内容
            int eventIdCount = eventIdList.Count;
            List<string> contentList = ParseContent(log);
            foreach (string content in contentList)
            {
                string[] subContents = StringTools.GetSubStringWithIndex(content);
                foreach (string subContent in subContents)
                {
                    if (string.IsNullOrWhiteSpace(subContent) || subContent.Length < 4 /*太短了, 不要*/)
                    {
                        continue;
                    }

                    if (eventIdCount > 0)
                    {
                        // 这里假设事件Id和内容的书写是一一对应的, 因此先把具有Id的changeList填满内容
                        foreach (ChangeItem change in changeList)
                        {
                            if (string.IsNullOrWhiteSpace(change.content))
                            {
                                --eventIdCount;
                                change.content = StringTools.TrimStringWithEventId(subContent, change.eventId);   // 去事件ID
                                change.content = StringTools.TrimStringWithIndex(change.content); // 去索引
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 这里加入的change将没有事件Id
                        ChangeItem change = new ChangeItem();
                        change.author = log.author;
                        change.revision = log.revision;
                        change.content = subContent;
                        change.content = StringTools.TrimStringWithIndex(change.content); // 去索引
                        change.log = log;
                        changeList.Add(change);
                    }
                }
            }

            // 目前changeList已经具备事件Id和内容, 可以用来解析类型
            foreach (ChangeItem change in changeList)
            {
                if (string.IsNullOrWhiteSpace(change.content))
                {
                    // 此时说明内容数据没有获取到, 直接用log的
                    change.content = change.log.content;
                }
                change.type = ParseChangeType(change);
            }
            return changeList;
        }

        /************************************************************************/
        /* 通用函数                                                             */
        /************************************************************************/
        private Constant.ChangeType ParseChangeType(ChangeItem change)
        {
            Constant.ChangeType type = Constant.ChangeType.Unknown;
            if (bool.Parse(ConfigurationManager.AppSettings[Constant.Cfg.EnableRegx]))
            {
                type = ParseTypeByRegx(change.content);
            }

            if (Constant.ChangeType.Unknown == type
                && bool.Parse(ConfigurationManager.AppSettings[Constant.Cfg.EnableAiDecisionChangeType]))
            {
                type = ParseTypeByNeuro(change.log.content);
            }

            return type;
        }

        
        private Changelog InsertChangelog(Changelog changelog, List<ChangeItem> changeList)
        {
            foreach (ChangeItem change in changeList)
            {
                List<ChangeItem> dstList = null;
                switch (change.type)
                {
                    case Constant.ChangeType.Add:
                        dstList = changelog.addList;
                        break;

                    case Constant.ChangeType.Back:
                        dstList = changelog.backList;
                        break;

                    case Constant.ChangeType.Optimize:
                        dstList = changelog.optimizeList;
                        break;

                    case Constant.ChangeType.Fix:
                        dstList = changelog.fixList;
                        break;

                    case Constant.ChangeType.Oem:
                        dstList = changelog.oemList;
                        break;

                    default:
                        dstList = changelog.unkownList;
                        break;
                }

                bool found = false;
                foreach (ChangeItem tmpChange in dstList)
                {
                    bool isSameContent = tmpChange.content.Length > change.content.Length
                        ? tmpChange.content.Contains(change.content) : change.content.Contains(tmpChange.content);
                    if ((!string.IsNullOrWhiteSpace(tmpChange.eventId)
                        && 0 == string.Compare(tmpChange.eventId, change.eventId, true))
                        || isSameContent)
                    {
                        found = true;
                        MergeChanges(tmpChange, change);
                        break;
                    }
                }

                if (found)
                {
                    continue;
                }

                dstList.Add(change);
            }

            return changelog;
        }

        private void MergeChanges(ChangeItem toChange, ChangeItem fromChange)
        {
            if (string.IsNullOrWhiteSpace(toChange.eventId)
                && !string.IsNullOrWhiteSpace(fromChange.eventId))
            {
                toChange.eventId = fromChange.eventId;
            }

            if (toChange.content.Length < fromChange.content.Length)
            {
                // 内容用长的, 更详细
                toChange.content = fromChange.content;
            }
        }

        private List<string> ParseEventId(LogItem log)
        {
            List<string> eventIdList = new List<string>();
            string eventId = null;
            try
            {
                string[] patterns = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxEventId]
                    .Split(Environment.NewLine.ToCharArray());

                foreach (string pattern in patterns)
                {
                    if (string.IsNullOrWhiteSpace(pattern))
                    {
                        continue;
                    }

                    MatchCollection matches = Regex.Matches(StringTools.TrimStringWithMergeInfo(log.content),
                        pattern.Trim(), RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (0 < matches.Count)
                    {
                        bool matched = false;
                        foreach (Match match in matches)
                        {
                            eventId = StringTools.TrimStringWithSpace(
                                match.Groups[Constant.Cfg.SvnRegxEventId].ToString())
                                .ToLower();
                            if (!string.IsNullOrWhiteSpace(eventId) && !eventIdList.Contains(eventId))
                            {
                                // 记录有效
                                matched = true;
                                eventIdList.Add(eventId);
                            }
                        }

                        if (matched)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return eventIdList;
        }

        private List<string> ParseContent(LogItem log)
        {
            List<string> contentList = new List<string>();
            string content = null;
            try
            {
                string[] patterns = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxContent]
                    .Split(Environment.NewLine.ToCharArray());

                foreach (string pattern in patterns)
                {
                    if (string.IsNullOrWhiteSpace(pattern))
                    {
                        continue;
                    }

                    MatchCollection matches = Regex.Matches(StringTools.TrimStringWithMergeInfo(log.content),
                        pattern.Trim(), RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (0 < matches.Count)
                    {
                        bool matched = false;
                        foreach (Match match in matches)
                        {
                            content = StringTools.TrimString(
                                match.Groups[Constant.Cfg.SvnRegxContent].ToString());
                            if (!string.IsNullOrWhiteSpace(content) 
                                && 0 != string.Compare(content, "无")
                                && !contentList.Contains(content))
                            {
                                // 记录有效
                                matched = true;
                                contentList.Add(content);
                            }
                        }

                        if (matched)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return contentList;
        }

        /************************************************************************/
        /* 正则处理相关函数                                                     */
        /************************************************************************/
        private Constant.ChangeType ParseTypeByRegx(string content)
        {
            // 设立投票标志, 哪种类型获取的票数多, 就倾向于这个类型
            Constant.ChangeType type = Constant.ChangeType.Unknown;
            int maxVotes = 0;
            int addVotes = 0, backVotes = 0, optimizeVotes = 0, fixVotes = 0, oemVotes = 0;

            // 优先级为Fix > Add > Optimize > Oem > Back
            fixVotes = GetVotes(ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxFix]
                    .Split(Environment.NewLine.ToCharArray()), content);
            if (fixVotes > maxVotes)
            {
                maxVotes = fixVotes;
                type = Constant.ChangeType.Fix;
            }

            addVotes = GetVotes(ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxAdd]
                    .Split(Environment.NewLine.ToCharArray()), content);
            if (addVotes > maxVotes)
            {
                maxVotes = addVotes;
                type = Constant.ChangeType.Add;
            }

            optimizeVotes = GetVotes(ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxOptimize]
                    .Split(Environment.NewLine.ToCharArray()), content);
            if (optimizeVotes > maxVotes)
            {
                maxVotes = optimizeVotes;
                type = Constant.ChangeType.Optimize;
            }

            oemVotes = GetVotes(ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxOem]
                    .Split(Environment.NewLine.ToCharArray()), content);
            if (oemVotes > maxVotes)
            {
                maxVotes = oemVotes;
                type = Constant.ChangeType.Oem;
            }

            backVotes = GetVotes(ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxBack]
                    .Split(Environment.NewLine.ToCharArray()), content);
            if (backVotes > maxVotes)
            {
                maxVotes = backVotes;
                type = Constant.ChangeType.Back;
            }

            return type;
        }

        private int GetVotes(string[] patterns, string content)
        {
            int votes = 0;
            try
            {
                foreach (string pattern in patterns)
                {
                    if (string.IsNullOrWhiteSpace(pattern))
                    {
                        continue;
                    }
                    pattern.Trim();

                    MatchCollection matches = Regex.Matches(content,
                        pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    votes += matches.Count;
                }
            }
            catch (Exception)
            {
                return 0;
            }

            return votes;
        }

        /************************************************************************/
        /* 神经网络相关                                                         */
        /************************************************************************/
        private LearnStatistics LoadLastStatistics()
        {
            LearnStatistics statistics = new LearnStatistics();
            long learnCnt = 0;
            double accuracy = 0.0;
            try
            {
                learnCnt = long.Parse(
                    ConfigurationManager.AppSettings[Constant.Cfg.StatisticsSvnChangeTypeCnt]);
                accuracy = double.Parse(
                    ConfigurationManager.AppSettings[Constant.Cfg.StatisticsSvnChangeTypeAccuracy]);
            }
            catch (Exception)
            {
                learnCnt = 0;
                accuracy = 0.0;
            }

            statistics.StatisticsChangeTypeCnt = learnCnt;
            statistics.StatisticsChangeTypeAccuracy = accuracy;
            return statistics;
        }

        private void SaveCurrentStatistics(LearnStatistics statistics)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.StatisticsSvnChangeTypeCnt].Value = 
                statistics.StatisticsChangeTypeCnt.ToString();
            config.AppSettings.Settings[Constant.Cfg.StatisticsSvnChangeTypeAccuracy].Value =
                statistics.StatisticsChangeTypeAccuracy.ToString();

            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void LearnChangeType(Changelog changelog, out long iterations, out double accuracy)
        {
            if (null == mChangeTypeNetwork)
            {
                // 建立网络
                mChangeTypeNetwork = CreateChangeTypeNetwork((int)Constant.ChangeType.Max);
            }

            // 构建训练数据
            double[][] inputData = null;
            double[][] outputData = null;
            BuildTrainChangeTypeData(changelog, out inputData, out outputData);

            // 开始训练
            accuracy = 1.0;
            iterations = 0;
            PerceptronLearning teacher = new PerceptronLearning(mChangeTypeNetwork);
            while (accuracy > Constant.Neuro.NeuronsError 
                && iterations < Constant.Neuro.NeuronsMaxIterations)
            {
                // teacher.RunEpoch returns absolute error
                accuracy = teacher.RunEpoch(inputData, outputData);
                ++iterations;
            }

            mChangeTypeNetwork.Save(ChangeTypeNeuro);

            // 精度等于1 - 误差
            accuracy = 1 - accuracy;
        }

        private ActivationNetwork CreateChangeTypeNetwork(int outputDimensions)
        {
            // 判断类型的神经网络
            return new ActivationNetwork(new ThresholdFunction(),
                    Constant.Neuro.NeuronsInputDimensions,
                    outputDimensions);
        }

        private ActivationNetwork CreateContentNetwork(int outputDimensions)
        {
            // 内容类型的神经网络
            return new ActivationNetwork(new BipolarSigmoidFunction(),
                    Constant.Neuro.NeuronsInputDimensions,
                    Constant.Neuro.NeuronsOutputLevelMiddle,
                    Constant.Neuro.NeuronsOutputLevelMiddle,
                    Constant.Neuro.NeuronsOutputLevelMiddle,
                    outputDimensions);
        }

        private double[] BuildInputDoubles(char[] contentBytes)
        {
            double[] inputDoubles = new double[Constant.Neuro.NeuronsInputDimensions];
            for (int i = 0; i < inputDoubles.Length; ++i)
            {
                // 归一化
                if (i < contentBytes.Length)
                {
                    inputDoubles[i] = ((double)contentBytes[i]) / 255;
                }
                else
                {
                    inputDoubles[i] = 0; 
                }
            }

            return inputDoubles;
        }

        private double[] BuildChangeTypeOutputDoubles(Constant.ChangeType type)
        {
            double[] outputDoubles = new double[(int)Constant.ChangeType.Max];
            for (int i = 0; i < outputDoubles.Length; ++i )
            {
                if (i == (int)type)
                {
                    outputDoubles[i] = 1.0;
                }
                else
                {
                    outputDoubles[i] = 0.0;
                }
            }
            return outputDoubles;
        }

        private void BuildTrainChangeTypeData(Changelog changelog, out double[][] inputData, out double[][] outputData)
        {
            List<double[]> inputList = new List<double[]>();
            List<double[]> outputList = new List<double[]>();

            // Add
            foreach (ChangeItem change in changelog.addList)
            {
                if (Constant.ChangeType.Add != change.type)
                {
                    continue;
                }

                inputList.Add(BuildInputDoubles(change.log.content.ToArray()));
                outputList.Add(BuildChangeTypeOutputDoubles(change.type));
            }

            // Back
            foreach (ChangeItem change in changelog.backList)
            {
                if (Constant.ChangeType.Back != change.type)
                {
                    continue;
                }

                inputList.Add(BuildInputDoubles(change.log.content.ToArray()));
                outputList.Add(BuildChangeTypeOutputDoubles(change.type));
            }

            // Optimize
            foreach (ChangeItem change in changelog.optimizeList)
            {
                if (Constant.ChangeType.Optimize != change.type)
                {
                    continue;
                }

                inputList.Add(BuildInputDoubles(change.log.content.ToArray()));
                outputList.Add(BuildChangeTypeOutputDoubles(change.type));
            }

            // Fix
            foreach (ChangeItem change in changelog.fixList)
            {
                if (Constant.ChangeType.Fix != change.type)
                {
                    continue;
                }

                inputList.Add(BuildInputDoubles(change.log.content.ToArray()));
                outputList.Add(BuildChangeTypeOutputDoubles(change.type));
            }

            // Oem
            foreach (ChangeItem change in changelog.oemList)
            {
                if (Constant.ChangeType.Oem != change.type)
                {
                    continue;
                }

                inputList.Add(BuildInputDoubles(change.log.content.ToArray()));
                outputList.Add(BuildChangeTypeOutputDoubles(change.type));
            }

            /*
            // Unknown
            foreach (ChangeItem change in changelog.unkownList)
            {
                if (Constant.ChangeType.Unknown != change.type)
                {
                    continue;
                }

                inputList.Add(BuildInputDoubles(change.log.content.ToArray()));
                outputList.Add(BuildChangeTypeOutputDoubles(change.type));
            }
            */

            inputData = inputList.ToArray();
            outputData = outputList.ToArray();
        }

        private Constant.ChangeType ParseTypeByNeuro(string content)
        {
            if (null == mChangeTypeNetwork)
            {
                return Constant.ChangeType.Unknown;
            }

            try
            {
                // 反归一化
                double[] doubleTypes = mChangeTypeNetwork.Compute(
                    BuildInputDoubles(content.ToArray()));

                double doubleMax = 0.0;
                int indexMax = 0;
                for (int i = 0; i < doubleTypes.Length; ++i)
                {
                    if (doubleTypes[i] > doubleMax)
                    {
                        doubleMax = doubleTypes[i];
                        indexMax = i;
                    }
                }

                return (Constant.ChangeType)indexMax;
            }
            catch (Exception)
            {
                return Constant.ChangeType.Unknown;
            }
        }
    }
}
