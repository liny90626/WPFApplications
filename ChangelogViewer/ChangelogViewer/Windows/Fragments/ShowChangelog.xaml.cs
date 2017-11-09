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

using ChangelogViewer.Definitions;

namespace ChangelogViewer.Windows.Fragments
{
    /// <summary>
    /// ShowChangelog.xaml 的交互逻辑
    /// </summary>
    public partial class ShowChangelog : UserControl, DataInterface
    {
        private MainWindow mWin = null;

        private Changelog mChangelog = null;

        public ShowChangelog(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public int SetData(Object data)
        {
            if (!(data is Changelog))
            {
                return -1;
            }

            mChangelog = data as Changelog;
            return 0;
        }

        public Object GetPrevData()
        {
            return null;
        }

        public Object GetNextData()
        {
            return null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.FirmwareVersion.Text = mChangelog.newVersionStr;
            this.SvnBranchName.Text = mChangelog.svnBranch;
            this.SvnRevision.Text = mChangelog.svnRevision;
            this.GitBranchName.Text = mChangelog.gitBranch;
            this.GitRevision.Text = mChangelog.gitRevision;

            this.AddList.ItemsSource = mChangelog.changeAddList;
            this.FixList.ItemsSource = mChangelog.changeFixList;
            this.OptList.ItemsSource = mChangelog.changeOptList;
            this.OemList.ItemsSource = mChangelog.changeOemList;
            this.OthList.ItemsSource = mChangelog.changeOthList;

            this.AddListHeader.IsExpanded = (mChangelog.changeAddList.Count > 0);
            this.FixListHeader.IsExpanded = (mChangelog.changeFixList.Count > 0);
            this.OptListHeader.IsExpanded = (mChangelog.changeOptList.Count > 0);
            this.OemListHeader.IsExpanded = (mChangelog.changeOemList.Count > 0);
            this.OthListHeader.IsExpanded = (mChangelog.changeOthList.Count > 0);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            mWin.Prev();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            mWin.SaveAndReload(mChangelog);
        }
    }
}
