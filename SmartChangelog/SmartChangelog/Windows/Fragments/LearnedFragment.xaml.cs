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
    /// LearnedFragment.xaml 的交互逻辑
    /// </summary>
    public partial class LearnedFragment : UserControl
    {
        private MainWindow mWin = null;

        private LearnStatistics mSvnStatistics = null;
        private LearnStatistics mGitStatistics = null;

        public LearnedFragment(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public void EnableAll()
        {
            this.SvnStatisticsChangeTypeAccuracy.IsEnabled = true;
            this.SvnStatisticsChangeTypeCnt.IsEnabled = true;
        }

        public void DisableAll()
        {
            this.SvnStatisticsChangeTypeAccuracy.IsEnabled = false;
            this.SvnStatisticsChangeTypeCnt.IsEnabled = false;
        }

        public void SetData(LearnStatistics svnStatistics, LearnStatistics gitStatistics)
        {
            mSvnStatistics = svnStatistics;
            mGitStatistics = gitStatistics;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != mSvnStatistics)
            {
                this.SvnStatisticsChangeTypeAccuracy.Text = mWin.FindResource("about") +
                    mSvnStatistics.StatisticsChangeTypeAccuracy.ToString("P");
                this.SvnStatisticsChangeTypeCnt.Text = mWin.FindResource("about") +
                    mSvnStatistics.StatisticsChangeTypeCnt.ToString()
                    + mWin.FindResource("unit_times");
            }
            else
            {
                this.SvnStatisticsChangeTypeAccuracy.Text = 
                    (string)mWin.FindResource("can_not_get_statistics");
                this.SvnStatisticsChangeTypeAccuracy.Text =
                    (string)mWin.FindResource("can_not_get_statistics");
            }
        }

    }
}
