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
using System.Windows.Media.Animation;

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

    public class ListViewItemStyleSelector : StyleSelector
    {

        private Dictionary<ListViewItem, List<Storyboard>> storyboards = new Dictionary<ListViewItem, List<Storyboard>>();

        /// <summary>
        /// 下面的示例演示如何定义一个为行定义 Style 的 StyleSelector。
        /// 此示例依据行索引定义 Background 颜色，为每行定义ListViewItem的动画板（Storyboard）。
        ///ListView控件在初始化的时候，每初始化一行ListViewItem的时候都会进入该函数
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            ListView listview =
                ItemsControl.ItemsControlFromItemContainer(container)
                as ListView;//获得当前ListView
            int index =
                listview.ItemContainerGenerator.IndexFromContainer(container);//行索引
            if (index % 2 == 0)
            {
                backGroundSetter.Value = Brushes.LightGray;
            }
            else
            {
                backGroundSetter.Value = Brushes.Transparent;
            }
            st.Setters.Add(backGroundSetter);

            return st;
        }
    }
}
