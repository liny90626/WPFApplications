using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

using Spire.Xls;

using ChangelogViewer.Definitions;
using System.Collections;

namespace ChangelogViewer.Windows.Fragments
{
    /// <summary>
    /// LoadingFile.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingChangelog : UserControl, DataInterface
    {
        private MainWindow mWin = null;

        private PickupInfo mPickupInfo = null;
        private Changelog mChangelog = null;  // 经过筛选的changelog

        private Task mLoadingTask = null;
        private CancellationTokenSource mCts = null;

        private ChangelogListCompare mListCmp = null;

        public LoadingChangelog(MainWindow win)
        {
            InitializeComponent();

            mWin = win;

            mListCmp = new ChangelogListCompare();
        }

        public int SetData(Object data)
        {
            if (!(data is PickupInfo))
            {
                return -1;
            }

            mPickupInfo = data as PickupInfo;
            return 0;
        }

        public Object GetPrevData()
        {
            return mPickupInfo.changelogList;
        }

        public Object GetNextData()
        {
            return mChangelog;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StartLoading();
        }

        private int StartLoading()
        {
            StopLoading();

            mCts = new CancellationTokenSource();
            mLoadingTask = new Task(() => Loading(mCts.Token, mPickupInfo), mCts.Token);
            mLoadingTask.Start();
            return 0;
        }

        public void StopLoading()
        {
            if (null == mCts)
            {
                return;
            }

            mCts.Cancel();
            mCts = null;
            mLoadingTask = null;
        }

        private void LoadingCompleted(bool success, string failedReason = null)
        {
            if (success)
            {
                mWin.Dispatcher.BeginInvoke(new Action(()
                    => mWin.Next()));
            }
            else
            {
                mWin.Dispatcher.BeginInvoke(new Action(()
                    =>
                    {
                        mWin.ShowTipMessage(failedReason);
                        mWin.Prev();
                    }));
            }
        }

        private void ShowLoadingState(string state)
        {
            mWin.Dispatcher.BeginInvoke(new Action(()
                =>
                {
                    this.LoadingState.Content = state;
                }));
        }

        private void Loading(CancellationToken ct, PickupInfo pickupInfo)
        {
            // 开始文件加载
            ShowLoadingState((string)mWin.FindResource("parsing_changelog"));

            int[] startVersion = ParseVersion(mPickupInfo.startVersion);
            int[] endVersion = ParseVersion(mPickupInfo.endVersion);
            if (null == startVersion || null == endVersion)
            {
                LoadingCompleted(false, (string)mWin.FindResource("parse_changelog_failed"));
                return;
            }

            if (startVersion[0] >= endVersion[0] && startVersion[1] > endVersion[1])
            {
                // 版本号选反了, 交换一下
                int tmp = startVersion[0];
                startVersion[0] = endVersion[0];
                endVersion[0] = tmp;

                tmp = startVersion[1];
                startVersion[1] = endVersion[1];
                endVersion[1] = tmp;
            }

            bool isSameVersion = (startVersion[0] == endVersion[0])
                && (startVersion[1] == endVersion[1]);

            if (isSameVersion)
            {
                LoadingCompleted(false, (string)mWin.FindResource("parse_changelog_same_version"));
                return;
            }

            mChangelog = BuildPickupDiffVersion(startVersion, endVersion, mPickupInfo.oemName);
            if (null == mChangelog)
            {
                LoadingCompleted(false, (string)mWin.FindResource("parse_changelog_failed"));
                return;
            }

            LoadingCompleted(true);
        }

        private int[] ParseVersion(string version)
        {
            string[] versions = version.Split('.');
            if (versions.Length != 2)
            {
                return null;
            }

            int[] versionInt = new int[versions.Length];
            for (int i = 0; i < versions.Length; ++i)
            {
                versionInt[i] = int.Parse(versions[i]);
            }

            return versionInt;
        }

        private Changelog BuildPickupDiffVersion(int[] startVersions, int[] endVersions, string oemName)
        {
            Changelog startChangelog = null, endChangelog = null;
            foreach (Changelog changelog in mPickupInfo.changelogList)
            {
                // 起始Changelog找最先出现的, 终止Changelog找最后出现的
                if (null == startChangelog &&
                    changelog.newVersionInt[2] == startVersions[0] &&
                    changelog.newVersionInt[3] == startVersions[1] &&
                    changelog.oemList.Contains(oemName))
                {
                    startChangelog = changelog;
                    continue;
                }

                if (changelog.newVersionInt[2] == endVersions[0] &&
                    changelog.newVersionInt[3] == endVersions[1] &&
                    changelog.oemList.Contains(oemName))
                {
                    endChangelog = changelog;
                }
            }

            if (null == startChangelog || null == endChangelog)
            {
                return null;
            }

            return BuildPickupChangelog(startChangelog, endChangelog);
        }

        private Changelog BuildPickupChangelog(Changelog startChangelog, Changelog endChangelog)
        {
            // 这里不需要考虑git分支的情况, 因为大部分情况下git是伴随着svn分支的
            bool isSameBranch = startChangelog.svnBranch.Equals(endChangelog.svnBranch, 
                StringComparison.OrdinalIgnoreCase);
            Changelog pickupChangelog = new Changelog(startChangelog, endChangelog);
            if (isSameBranch)
            {
                // 同分支, 直接找revision区间内的修改即可
                foreach (Changelog changelog in mPickupInfo.changelogList)
                {
                    if (IsChangelogNeedRecorded(changelog, startChangelog, endChangelog))
                    {
                        pickupChangelog.changeAddList = pickupChangelog.changeAddList
                            .Union(changelog.changeAddList, mListCmp).ToList<ChangeRecord>();
                        pickupChangelog.changeFixList = pickupChangelog.changeFixList
                            .Union(changelog.changeFixList, mListCmp).ToList<ChangeRecord>();
                        pickupChangelog.changeOptList = pickupChangelog.changeOptList
                            .Union(changelog.changeOptList, mListCmp).ToList<ChangeRecord>();
                        pickupChangelog.changeOemList = pickupChangelog.changeOemList
                            .Union(changelog.changeOemList, mListCmp).ToList<ChangeRecord>();
                        pickupChangelog.changeOthList = pickupChangelog.changeOthList
                            .Union(changelog.changeOthList, mListCmp).ToList<ChangeRecord>();
                    }
                }
            }
            else
            {
                // 不同分支, 找相同的两段分支
                Changelog startChangelogSameBranchMax = null, endChangelogSameBranchMin = null; ;
                foreach (Changelog changelog in mPickupInfo.changelogList)
                {
                    if (changelog.newVersionInt[2] >= startChangelog.newVersionInt[2]
                        && changelog.newVersionInt[3] > startChangelog.newVersionInt[3]
                        && changelog.svnBranch.Equals(startChangelog.svnBranch, StringComparison.OrdinalIgnoreCase))
                    {
                        startChangelogSameBranchMax = changelog;
                        continue;
                    }

                    if (null == endChangelogSameBranchMin
                        && changelog.newVersionInt[2] <= endChangelog.newVersionInt[2]
                        && changelog.newVersionInt[3] < endChangelog.newVersionInt[3]
                        && changelog.svnBranch.Equals(endChangelog.svnBranch, StringComparison.OrdinalIgnoreCase))
                    {
                        endChangelogSameBranchMin = changelog;
                    }
                }

                if (null != startChangelogSameBranchMax)
                {
                    // 递归一级
                    startChangelogSameBranchMax = BuildPickupChangelog(startChangelog, startChangelogSameBranchMax);
                    pickupChangelog.changeAddList = pickupChangelog.changeAddList
                        .Union(startChangelogSameBranchMax.changeAddList, mListCmp).ToList<ChangeRecord>();
                    pickupChangelog.changeFixList = pickupChangelog.changeFixList
                        .Union(startChangelogSameBranchMax.changeFixList, mListCmp).ToList<ChangeRecord>();
                    pickupChangelog.changeOptList = pickupChangelog.changeOptList
                        .Union(startChangelogSameBranchMax.changeOptList, mListCmp).ToList<ChangeRecord>();
                    pickupChangelog.changeOemList = pickupChangelog.changeOemList
                        .Union(startChangelogSameBranchMax.changeOemList, mListCmp).ToList<ChangeRecord>();
                    pickupChangelog.changeOthList = pickupChangelog.changeOthList
                        .Union(startChangelogSameBranchMax.changeOthList, mListCmp).ToList<ChangeRecord>();
                }
                else
                {
                    startChangelogSameBranchMax = startChangelog;
                }

                if (null != endChangelogSameBranchMin)
                {
                    // 递归一级
                    endChangelogSameBranchMin = BuildPickupChangelog(endChangelogSameBranchMin, endChangelog);
                }
                else
                {
                    endChangelogSameBranchMin = endChangelog;
                }

                pickupChangelog.changeAddList = pickupChangelog.changeAddList
                    .Union(endChangelogSameBranchMin.changeAddList, mListCmp).ToList<ChangeRecord>();
                pickupChangelog.changeFixList = pickupChangelog.changeFixList
                    .Union(endChangelogSameBranchMin.changeFixList, mListCmp).ToList<ChangeRecord>();
                pickupChangelog.changeOptList = pickupChangelog.changeOptList
                    .Union(endChangelogSameBranchMin.changeOptList, mListCmp).ToList<ChangeRecord>();
                pickupChangelog.changeOemList = pickupChangelog.changeOemList
                    .Union(endChangelogSameBranchMin.changeOemList, mListCmp).ToList<ChangeRecord>();
                pickupChangelog.changeOthList = pickupChangelog.changeOthList
                    .Union(endChangelogSameBranchMin.changeOthList, mListCmp).ToList<ChangeRecord>();
            }

            return pickupChangelog;
        }

        private bool IsChangelogNeedRecorded(Changelog curChangelog, Changelog startChangelog, Changelog endChangelog)
        {
            // 所有分支必须相同
            Changelog lastChangelog = null;
            foreach (Changelog changelog in mPickupInfo.changelogList)
            {
                if (curChangelog.oldVersionStr.Equals(changelog.newVersionStr, StringComparison.OrdinalIgnoreCase)
                    && curChangelog.svnBranch.Equals(changelog.svnBranch, StringComparison.OrdinalIgnoreCase))
                {
                    lastChangelog = changelog;
                    break;
                }
            }

            if (null == lastChangelog)
            {
                return false;
            }

            if (!IsRevisionInside(lastChangelog.svnRevision, 
                startChangelog.svnRevision, endChangelog.svnRevision))
            {
                return false;
            }

            if (!IsRevisionInside(curChangelog.svnRevision,
                startChangelog.svnRevision, endChangelog.svnRevision))
            {
                return false;
            }

            return true;
        }

        private bool IsRevisionInside(string curRevision, string startRevison, string endRevision)
        {
            int curRevisionInt = -1, startRevisonInt = -1, endRevisionInt = -1;
            try
            {
                curRevisionInt = curRevision.Contains('#') ? 
                    int.Parse(curRevision.Substring(curRevision.IndexOf('#') + 1))
                    : int.Parse(curRevision);  
            }
            catch (System.Exception)
            {
                // 异常了, 无法继续
                return false;
            }

            try
            {
                startRevisonInt = startRevison.Contains('#') ?
                    int.Parse(startRevison.Substring(startRevison.IndexOf('#') + 1))
                    : int.Parse(startRevison);
            }
            catch (System.Exception)
            {
                // 异常了, 无法继续
                return false;
            }

            try
            {
                endRevisionInt = endRevision.Contains('#') ?
                    int.Parse(endRevision.Substring(endRevision.IndexOf('#') + 1))
                    : int.Parse(endRevision);
            }
            catch (System.Exception)
            {
                // 异常了, 无法继续
                return false;
            }
            

            if (startRevisonInt <= curRevisionInt
                && endRevisionInt >= curRevisionInt)
            {
                return true;
            }

            return false;
        }
    }

    public class ChangelogListCompare : IEqualityComparer<ChangeRecord>
    {
        public bool Equals(ChangeRecord x, ChangeRecord y)
        {
            if (x.tested == y.tested
                && x.change.Equals(y.change, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(ChangeRecord obj)
        {
            return obj.change.GetHashCode();
        }
    }
}
