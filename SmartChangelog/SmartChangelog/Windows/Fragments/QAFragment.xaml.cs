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

using SmartChangelog.Definitions;
using SmartChangelog.Tools;

namespace SmartChangelog.Windows.Fragments
{
    /// <summary>
    /// QAFragment.xaml 的交互逻辑
    /// </summary>
    public partial class QAFragment : UserControl
    {
        private MainWindow mWin = null;

        /* Data */
        private Changelog mSvnChangelog = null;
        private Changelog mGitChangelog = null;
        private int mDataIndex = 0;
        private int mDataSize = 0;
        private int mGitOffset = 0;

        public QAFragment(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public void EnableAll()
        {
            this.ChangeEventId.IsEnabled = true;
            this.ChangeContent.IsEnabled = true;
            this.ChangeType.IsEnabled = true;
            this.LogContent.IsEnabled = true;
        }

        public void DisableAll()
        {
            this.ChangeEventId.IsEnabled = false;
            this.ChangeContent.IsEnabled = false;
            this.ChangeType.IsEnabled = false;
            this.LogContent.IsEnabled = false;
        }

        public void SetData(Changelog svnChangelog, Changelog gitChangelog)
        {
            mSvnChangelog = svnChangelog;
            mGitChangelog = gitChangelog;

            // 设置数据, 索引归零
            mDataIndex = 0;
        }

        public void GetData(out Changelog svnChangelog, out Changelog gitChangelog)
        {
            svnChangelog = mSvnChangelog;
            gitChangelog = mGitChangelog;
        }

        public void ShowNextData()
        {
            if (IsTail())
            {
                return;
            }

            // 当跳转下一个时, 将当前修改保存
            ChangeItem change = GetCurrentData();
            change.eventId = StringTools.TrimStringWithSpace(
                this.ChangeEventId.Text.Trim().ToLower());
            change.content = StringTools.TrimStringWithEventId(
                this.ChangeContent.Text.Trim(), change.eventId);
            change.type = (Constant.ChangeType)this.ChangeType.SelectedIndex;

            ++mDataIndex;
            ShowData();
        }

        public void ShowPrevData()
        {
            if (IsHead())
            {
                return;
            }

            --mDataIndex;
            ShowData();
        }

        public bool IsHead()
        {
            return (mDataIndex == 0);
        }

        public bool IsTail()
        {
            return (mDataIndex >= (mDataSize - 1));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> changeTypes = new List<string>();
            changeTypes.Add((string)mWin.FindResource("others"));    // unknown对应其他
            changeTypes.Add((string)mWin.FindResource("add"));
            changeTypes.Add((string)mWin.FindResource("back"));
            changeTypes.Add((string)mWin.FindResource("optimize"));
            changeTypes.Add((string)mWin.FindResource("fix"));
            changeTypes.Add((string)mWin.FindResource("oem"));
            this.ChangeType.ItemsSource = changeTypes;
            this.ChangeType.SelectedIndex = (int)Constant.ChangeType.Unknown;

            // 每次加载时都从索引0开始
            mDataIndex = 0;

            // 计算数据总长度, svn默认优先于git显示
            mDataSize = 0;
            mGitOffset = 0;
            if (null != mSvnChangelog)
            {
                mDataSize += mSvnChangelog.unkownList.Count;
                mGitOffset = mSvnChangelog.unkownList.Count;
            }

            if (null != mGitChangelog)
            {
                mDataSize += mGitChangelog.unkownList.Count;
            }

            ShowData();
        }

        private void ShowData()
        {
            ChangeItem change = GetCurrentData();
            this.ChangeEventId.Text = change.eventId;
            this.ChangeContent.Text = change.content;
            this.ChangeType.SelectedIndex = (int)change.type;
            this.ChangeAuthor.Text = change.author;
            this.ChangeRevision.Text = change.revision;
            this.LogContent.Text = change.log.content;

            this.ListSizeAndIndex.Content = (mDataIndex + 1) + "/" + mDataSize;
        }

        private ChangeItem GetCurrentData()
        {
            ChangeItem change = null;
            if (mDataIndex < mGitOffset)
            {
                // SVN
                change = mSvnChangelog.unkownList[mDataIndex];
            }
            else
            {
                // Git
                change = mGitChangelog.unkownList[mDataIndex - mGitOffset];
            }

            return change;
        }

    }
}
