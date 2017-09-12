using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

using SmartChangelog.Definitions;

namespace SmartChangelog.Controls
{
    class MainControl
    {
        private MainWindow mWin = null;

        private SvnControl mSvnC = null;
        private GitControl mGitC = null;

        private Task mLoadTask = null;
        private CancellationTokenSource mLoadCts = null;

        private Task mReportTask = null;
        private CancellationTokenSource mReportCts = null;

        private Task mLearnTask = null;
        private CancellationTokenSource mLearnCts = null;

        public MainControl(MainWindow win)
        {
            mWin = win;

            mSvnC = new SvnControl(win);
            mGitC = new GitControl(win);
        }

        public void LoadDataAsync(Dictionary<Constant.DictName, string> dict)
        {
            // 停止上一次任务(若存在)
            StopLoadTask();

            mLoadCts = new CancellationTokenSource();
            mLoadTask = new Task(() => LoadData(mLoadCts.Token, dict), mLoadCts.Token);
            mLoadTask.Start();
        }

        public void StopLoadTask()
        {
            if (null != mLoadCts)
            {
                mLoadCts.Cancel();
                mLoadCts = null;
                mLoadTask = null;
            }
        }

        public void ReportDataAsync(Changelog svnChangelog, Changelog gitChangelog)
        {
            // 停止上一次任务(若存在)
            StopReportTask();

            mReportCts = new CancellationTokenSource();
            mReportTask = new Task(() => ReportData(mReportCts.Token, svnChangelog, gitChangelog), mReportCts.Token);
            mReportTask.Start();
        }

        public void StopReportTask()
        {
            if (null != mReportCts)
            {
                mReportCts.Cancel();
                mReportCts = null;
                mReportTask = null;
            }
        }

        public void LearnDataAsync(Changelog svnChangelog, Changelog gitChangelog)
        {
            // 停止上一次任务(若存在)
            StopLearnTask();

            mLearnCts = new CancellationTokenSource();
            mLearnTask = new Task(() => LearnData(mLearnCts.Token, svnChangelog, gitChangelog), mLearnCts.Token);
            mLearnTask.Start();
        }

        public void StopLearnTask()
        {
            if (null != mLearnCts)
            {
                mLearnCts.Cancel();
                mLearnCts = null;
                mLearnTask = null;
            }
        }

        private void LoadData(CancellationToken ct, Dictionary<Constant.DictName, string> dict)
        {
            bool doAction = false;
            bool success = false;
            string err = null;
            Changelog svnChangelog = null;
            Changelog gitChangelog = null;
            if (IsSvnReady(dict))
            {
                ReportProgress((string)mWin.FindResource("svn_log_fetching"));
                doAction = true;

                long startVersion = long.Parse(dict[Constant.DictName.SvnLastVersion]);
                long endVersion = long.Parse(dict[Constant.DictName.SvnCurrentVersion]);
                List<LogItem> logList = mSvnC.GetSvnLogList(
                    ConfigurationManager.AppSettings[Constant.Cfg.SvnServerAddr], 
                    dict[Constant.DictName.SvnBranchName], 
                    Math.Min(startVersion, endVersion),
                    Math.Max(startVersion, endVersion), 
                    out success, out err);
                if (!success)
                {
                    goto exit;
                }

                ReportProgress((string)mWin.FindResource("svn_log_analyzing"));
                svnChangelog = mSvnC.AnalyzeLog(logList, out success, out err);
                if (!success)
                {
                    goto exit;
                }
            }

            if (!doAction)
            {
                success = false;
                err = (string)mWin.FindResource("do_nothing_please_check");
                goto exit;
            }

        exit:
            NotifyLoadingFinished(success, err, svnChangelog, gitChangelog);
        }

        private void ReportData(CancellationToken ct, Changelog svnChangelog, Changelog gitChangelog)
        {
            ReportProgress((string)mWin.FindResource("reporting"));
            Changelog allChangelog = new Changelog();

            // 新增
            if (null != svnChangelog)
            {
                GenerateReportChangelog(svnChangelog.addList, 
                    allChangelog.addList, Constant.ChangeType.Add);
                GenerateReportChangelog(svnChangelog.unkownList,
                    allChangelog.addList, Constant.ChangeType.Add);
            }
            if (null != gitChangelog)
            {
                GenerateReportChangelog(gitChangelog.addList, 
                    allChangelog.addList, Constant.ChangeType.Add);
                GenerateReportChangelog(gitChangelog.unkownList,
                    allChangelog.addList, Constant.ChangeType.Add);
            }

            // 回退
            if (null != svnChangelog)
            {
                GenerateReportChangelog(svnChangelog.backList, 
                    allChangelog.backList, Constant.ChangeType.Back);
                GenerateReportChangelog(svnChangelog.unkownList,
                    allChangelog.backList, Constant.ChangeType.Back);
            }
            if (null != gitChangelog)
            {
                GenerateReportChangelog(gitChangelog.backList, 
                    allChangelog.backList, Constant.ChangeType.Back);
                GenerateReportChangelog(gitChangelog.unkownList,
                    allChangelog.backList, Constant.ChangeType.Back);
            }

            // 优化
            if (null != svnChangelog)
            {
                GenerateReportChangelog(svnChangelog.optimizeList,
                    allChangelog.optimizeList, Constant.ChangeType.Optimize);
                GenerateReportChangelog(svnChangelog.unkownList,
                    allChangelog.optimizeList, Constant.ChangeType.Optimize);
            }
            if (null != gitChangelog)
            {
                GenerateReportChangelog(gitChangelog.optimizeList,
                    allChangelog.optimizeList, Constant.ChangeType.Optimize);
                GenerateReportChangelog(gitChangelog.unkownList,
                   allChangelog.optimizeList, Constant.ChangeType.Optimize);
            }

            // 修复
            if (null != svnChangelog)
            {
                GenerateReportChangelog(svnChangelog.fixList,
                    allChangelog.fixList, Constant.ChangeType.Fix);
                GenerateReportChangelog(svnChangelog.unkownList,
                    allChangelog.fixList, Constant.ChangeType.Fix);
            }
            if (null != gitChangelog)
            {
                GenerateReportChangelog(gitChangelog.fixList,
                    allChangelog.fixList, Constant.ChangeType.Fix);
                GenerateReportChangelog(gitChangelog.unkownList,
                    allChangelog.fixList, Constant.ChangeType.Fix);
            }

            // 定制
            if (null != svnChangelog)
            {
                GenerateReportChangelog(svnChangelog.oemList,
                    allChangelog.oemList, Constant.ChangeType.Oem);
                GenerateReportChangelog(svnChangelog.unkownList,
                    allChangelog.oemList, Constant.ChangeType.Oem);
            }
            if (null != gitChangelog)
            {
                GenerateReportChangelog(gitChangelog.oemList,
                    allChangelog.oemList, Constant.ChangeType.Oem);
                GenerateReportChangelog(gitChangelog.unkownList,
                    allChangelog.oemList, Constant.ChangeType.Oem);
            }

            // 其他
            if (null != svnChangelog)
            {
                GenerateReportChangelog(svnChangelog.unkownList,
                    allChangelog.unkownList, Constant.ChangeType.Unknown);
            }
            if (null != gitChangelog)
            {
                GenerateReportChangelog(gitChangelog.unkownList,
                    allChangelog.unkownList, Constant.ChangeType.Unknown);
            }

            NotifyReportFinished(true, null, allChangelog, svnChangelog, gitChangelog);
        }

        private void LearnData(CancellationToken ct, Changelog svnChangelog, Changelog gitChangelog)
        {
            ReportProgress((string)mWin.FindResource("learning"));
            
            // 训练Svn神经网络
            LearnStatistics svnStatistics = null;
            mSvnC.LearnData(svnChangelog, out svnStatistics);

            // 训练Git神经网络
            LearnStatistics gitStatistics = null;
            mGitC.LearnData(gitChangelog, out gitStatistics);

            NotifyLearnFinished(true, null, svnStatistics, gitStatistics);
        }

        private bool IsSvnReady(Dictionary<Constant.DictName, string> dict)
        {
            long tmpOut;
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[Constant.Cfg.SvnServerAddr])
                || string.IsNullOrWhiteSpace(dict[Constant.DictName.SvnBranchName])
                || !long.TryParse(dict[Constant.DictName.SvnLastVersion], out tmpOut)
                || !long.TryParse(dict[Constant.DictName.SvnCurrentVersion], out tmpOut))
            {
                return false;
            }

            return true;
        }

        private void GenerateReportChangelog(List<ChangeItem> srcChangeList, List<ChangeItem> dstChangeList, Constant.ChangeType changeType)
        {
            foreach (ChangeItem srcChange in srcChangeList)
            {
                bool contained = false;
                foreach (ChangeItem dstChange in dstChangeList)
                {
                    if (0 == String.Compare(dstChange.content, srcChange.content))
                    {
                        // eventid一样并不能说明两次改变是相同的, 有可能一个eventid对应多个改变
                        contained = true;
                        break;
                    }
                }

                if (!contained && srcChange.type == changeType)
                {
                    dstChangeList.Add(srcChange);
                }
            }
        }

        private void ReportProgress(string state)
        {
            mWin.ReportLoadingProgressAsync(state);
        }

        private void NotifyLoadingFinished(bool success, string err, Changelog svnChangelog, Changelog gitChangelog)
        {
            StopLoadTask();
            mWin.NotifyLoadingFinishedAsync(success, err, svnChangelog, gitChangelog);
        }

        private void NotifyReportFinished(bool success, string err,
            Changelog allChangelog, Changelog svnChangelog, Changelog gitChangelog)
        {
            StopReportTask();
            mWin.NotifyReportFinishedAsync(success, err, allChangelog, svnChangelog, gitChangelog);
        }

        private void NotifyLearnFinished(bool success, string err, 
            LearnStatistics svnStatistics, LearnStatistics gitStatistics)
        {
            StopLearnTask();
            mWin.NotifyLearnFinishedAsync(success, err, svnStatistics, gitStatistics);
        }
    }
}
