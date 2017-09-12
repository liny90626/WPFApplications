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

namespace SmartChangelog.Windows.Fragments
{
    /// <summary>
    /// ReportFragment.xaml 的交互逻辑
    /// </summary>
    public partial class ReportFragment : UserControl
    {
        private MainWindow mWin = null;

        private Changelog mAllChangelog = null;
        private Changelog mSvnChangelog = null;
        private Changelog mGitChangelog = null;

        private string mReportChangelog = null;
        private string mBlameChangelog = null;

        public ReportFragment(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public void EnableAll()
        {
            this.ReportChangelog.IsEnabled = true;
            this.BlameChangelog.IsEnabled = true;
        }

        public void DisableAll()
        {
            this.ReportChangelog.IsEnabled = false;
            this.BlameChangelog.IsEnabled = false;
        }

        public void SetData(Changelog allChangelog, Changelog svnChangelog, Changelog gitChangelog)
        {
            mAllChangelog = allChangelog;
            mSvnChangelog = svnChangelog;
            mGitChangelog = gitChangelog;

            // 此时还在loading阶段, 顺带准备一下要显示的数据
            mReportChangelog = GenerateReport(allChangelog);
            mBlameChangelog = GenerateBlame(svnChangelog, gitChangelog);
        }

        public void GetData(out Changelog allChangelog)
        {
            allChangelog = mAllChangelog;
        }

        public void GetData(out Changelog svnChangelog, out Changelog gitChangelog)
        {
            svnChangelog = mSvnChangelog;
            gitChangelog = mGitChangelog;
        }

        public void GetData(out Changelog allChangelog, out Changelog svnChangelog, out Changelog gitChangelog)
        {
            allChangelog = mAllChangelog;
            svnChangelog = mSvnChangelog;
            gitChangelog = mGitChangelog;
        }

        private string GenerateReport(Changelog allChangelog)
        {
            // 优先级为Fix > Add > Optimize > Oem > Back
            string content = "";
            bool isFirstLine = true;

            // 修复
            if (0 < allChangelog.fixList.Count)
            {
                if (!isFirstLine)
                {
                    content += Environment.NewLine;
                }
                isFirstLine = false;

                content += mWin.FindResource("fix")
                    + ":" + Environment.NewLine
                    + GenerateSubReport(allChangelog.fixList);
            }

            // 新增
            if (0 < allChangelog.addList.Count)
            {
                if (!isFirstLine) 
                {
                    content += Environment.NewLine;
                }
                isFirstLine = false;

                content += mWin.FindResource("add")
                    + ":" + Environment.NewLine
                    + GenerateSubReport(allChangelog.addList);
            }

            // 优化
            if (0 < allChangelog.optimizeList.Count)
            {
                if (!isFirstLine)
                {
                    content += Environment.NewLine;
                }
                isFirstLine = false;

                content += mWin.FindResource("optimize")
                    + ":" + Environment.NewLine
                    + GenerateSubReport(allChangelog.optimizeList);
            }

            // 定制
            if (0 < allChangelog.oemList.Count)
            {
                if (!isFirstLine)
                {
                    content += Environment.NewLine;
                }
                isFirstLine = false;

                content += mWin.FindResource("oem")
                    + ":" + Environment.NewLine
                    + GenerateSubReport(allChangelog.oemList);
            }

            // 回退
            if (0 < allChangelog.backList.Count)
            {
                if (!isFirstLine)
                {
                    content += Environment.NewLine;
                }
                isFirstLine = false;

                content += mWin.FindResource("back") 
                    + ":" + Environment.NewLine
                    + GenerateSubReport(allChangelog.backList);
            }

            
            // 其他
            if (0 < allChangelog.unkownList.Count)
            {
                if (!isFirstLine)
                {
                    content += Environment.NewLine;
                }
                isFirstLine = false;

                content += mWin.FindResource("others")
                    + ":" + Environment.NewLine
                    + GenerateSubReport(allChangelog.unkownList);
            }

            return content;
        }

        private string GenerateSubReport(List<ChangeItem> list)
        {
            string content = "";
            int index = 1;
            foreach (ChangeItem change in list)
            {
                if (!string.IsNullOrWhiteSpace(change.eventId))
                {
                    content += index + ". (" + change.eventId + ")" + change.content;
                }
                else
                {
                    content += index + ". " + change.content;
                }

                ++index;
                content += Environment.NewLine;
            }

            return content;
        }

        private string GenerateBlame(Changelog svnChangelog, Changelog gitChangelog)
        {
            string content = "";
            if (null != svnChangelog)
            {
                content += "####################### Svn #######################";
                content += Environment.NewLine;
                foreach (ChangeItem change in svnChangelog.unkownList)
                {
                    content += mWin.FindResource("author") + ": " + change.author;
                    content += "    ";
                    content += mWin.FindResource("revision") + ": " + change.revision;
                    content += Environment.NewLine;
                }
            }

            if (null != gitChangelog)
            {
                content += "####################### Git #######################";
                content += Environment.NewLine;
                foreach (ChangeItem change in gitChangelog.unkownList)
                {
                    content += mWin.FindResource("author") + ": " + change.author;
                    content += "    ";
                    content += mWin.FindResource("revision") + ": " + change.revision;
                    content += Environment.NewLine;
                }
            }

            return content;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ReportChangelog.Text = mReportChangelog;
            this.BlameChangelog.Text = mBlameChangelog;
        }
    }
}
