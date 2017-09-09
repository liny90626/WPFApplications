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

namespace SmartChangelog.Controls
{
    class SvnControl
    {
        private const string SVN_ADMIN_USERNAME = "jenkins_admin";
        private const string SVN_ADMIN_PASSWORD = "jenkins@@)!&0904";

        private MainWindow mWin = null;

        public SvnControl(MainWindow win)
        {
            mWin = win;
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
                    if (string.IsNullOrWhiteSpace(subContent))
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
                change.type = ParseType(change.content);
            }
            return changeList;
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
                        };
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

        private Constant.ChangeType ParseType(string content)
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
    }
}
