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
    /// PickupVersion.xaml 的交互逻辑
    /// </summary>
    public partial class PickupVersion : UserControl, DataInterface
    {
        private MainWindow mWin = null;

        private PickupInfo mPickupInfo = null;
        private List<Changelog> mChangelogList = null;

        public PickupVersion(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public int SetData(Object data)
        {
            if (!(data is List<Changelog>))
            {
                return -1;
            }

            mChangelogList = data as List<Changelog>;
            return 0;
        }

        public Object GetPrevData()
        {
            return null;
        }

        public Object GetNextData()
        {
            return mPickupInfo;
        }

        private void ShowTipMessage(string tips)
        {
            mWin.Dispatcher.BeginInvoke(new Action(()
                =>
            {
                mWin.ShowTipMessage(tips);
            }));
        }

        private List<string> BuildOemNameList()
        {
            List<string> oemNameList = new List<string>();
            if (null == mChangelogList)
            {
                return oemNameList;
            }

            foreach (Changelog changelog in mChangelogList)
            {
                oemNameList = oemNameList.Union(changelog.oemList).ToList<string>();
            }

            return oemNameList;
        }

        private List<string> BuildVersionList(string oemName)
        {
            List<string> versionList = new List<string>();
            if (null == mChangelogList || string.IsNullOrWhiteSpace(oemName))
            {
                return versionList;
            }

            foreach (Changelog changelog in mChangelogList)
            {
                if (changelog.oemList.Contains(oemName))
                {
                    versionList.Add(changelog.newVersionInt[2] 
                        + "." + changelog.newVersionInt[3]);
                }
            }

            return versionList.Distinct<string>().ToList<string>();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.OemName.ItemsSource = BuildOemNameList();
            this.OemName.IsEnabled = true;

            // 未选择oem之前, 版本是无法确定的, 因此需要禁用
            if (string.IsNullOrWhiteSpace(this.OemName.Text))
            {
                this.StartVersion.IsEnabled = false;
                this.EndVersion.IsEnabled = false;
            }
            else
            {
                this.StartVersion.IsEnabled = true;
                this.EndVersion.IsEnabled = true;
            }
        }

        private void OemName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == (sender as ComboBox).SelectedItem)
            {
                this.StartVersion.IsEnabled = false;
                this.EndVersion.IsEnabled = false;
                return;
            }

            string text = (sender as ComboBox).SelectedItem.ToString();

            this.StartVersion.IsEnabled = true;
            this.EndVersion.IsEnabled = true;

            this.StartVersion.ItemsSource = BuildVersionList(text);
            this.EndVersion.ItemsSource = this.StartVersion.ItemsSource;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string oemName = this.OemName.Text;
            string startVersion = this.StartVersion.Text;
            string endVersion = this.EndVersion.Text;
            if (string.IsNullOrWhiteSpace(oemName) ||
                string.IsNullOrWhiteSpace(startVersion) ||
                string.IsNullOrWhiteSpace(endVersion))
            {
                ShowTipMessage((string)mWin.FindResource("bad_parameters"));
                return;
            }

            mPickupInfo = new PickupInfo();
            mPickupInfo.oemName = oemName;
            mPickupInfo.startVersion = startVersion;
            mPickupInfo.endVersion = endVersion;
            mPickupInfo.changelogList = mChangelogList;

            mWin.Next();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            mWin.Prev();
        }
    }
}
