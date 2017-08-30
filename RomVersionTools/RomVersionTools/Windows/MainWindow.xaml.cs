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
using System.Windows.Forms;
using System.IO;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RomVersionTools.Defines;
using RomVersionTools.Tools;

namespace RomVersionTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Constants.UiState mUiState = Constants.UiState.Idle;

        private DataTools mDataT = null;

        private Task mLoadTask = null;
        private CancellationTokenSource mLoadCts = null;

        private Task mSaveTask = null;
        private CancellationTokenSource mSaveCts = null;

        private List<VersionNodeItem> mDataTree = null;

        private string mStrFolderPath = null;

        public MainWindow()
        {
            InitializeComponent();

            // 版本号, [大版本].[小版本].[与2000/1/1的差值天数].[当天时间刻度]
            this.VersionLabel.Content = (string)FindResource("version") + ": "
                + App.ResourceAssembly.GetName(false).Version;

            mDataT = new DataTools();
            ShowState();
        }

        ~MainWindow()
        {
        }

        private void ShowState()
        {
            switch (mUiState) 
            {
                case Constants.UiState.DataSaving:
                    {
                        this.RomVersionTreeLoading.Visibility = Visibility.Visible;
                        this.RomVersionTree.Visibility = Visibility.Collapsed;

                        this.LoadBtn.IsEnabled = false;
                        this.LoadBtn.Content = (string)FindResource("loaded");

                        this.ResetBtn.IsEnabled = true;

                        this.SaveBtn.IsEnabled = false;
                        this.SaveBtn.Content = (string)FindResource("saving");
                    }
                    break;

                case Constants.UiState.DataLoaded:
                    {
                        this.RomVersionTreeLoading.Visibility = Visibility.Collapsed;
                        this.RomVersionTree.Visibility = Visibility.Visible;

                        this.LoadBtn.IsEnabled = false;
                        this.LoadBtn.Content = (string)FindResource("loaded");

                        this.ResetBtn.IsEnabled = true;

                        this.SaveBtn.IsEnabled = true;
                        this.SaveBtn.Content = (string)FindResource("save");
                    }
                    break;

                case Constants.UiState.DataLoading:
                    {
                        this.RomVersionTreeLoading.Visibility = Visibility.Visible;
                        this.RomVersionTree.Visibility = Visibility.Collapsed;

                        this.LoadBtn.IsEnabled = false;
                        this.LoadBtn.Content = (string)FindResource("loading");

                        this.ResetBtn.IsEnabled = true;

                        this.SaveBtn.IsEnabled = false;
                        this.SaveBtn.Content = (string)FindResource("save");
                    }
                    break;

                case Constants.UiState.Idle:
                default:
                    {
                        this.RomVersionTreeLoading.Visibility = Visibility.Collapsed;
                        this.RomVersionTree.Visibility = Visibility.Visible;

                        this.LoadBtn.IsEnabled = true;
                        this.LoadBtn.Content = (string)FindResource("load");

                        this.ResetBtn.IsEnabled = true;

                        this.SaveBtn.IsEnabled = false;
                        this.SaveBtn.Content = (string)FindResource("save");
                    }
                    break;
            }
        }

        private void showTipMessage(string message)
        {
            this.ShowMessageAsync((string)FindResource("tips"),
                message, MessageDialogStyle.Affirmative,
                new MetroDialogSettings() { AffirmativeButtonText = (string)FindResource("ok") });
        }

        private void notifyLoadingFinshed(bool success, List<VersionNodeItem> dataTree)
        {
            // 执行结束
            mLoadCts = null;
            mLoadTask = null;

            if (!success || null == dataTree || 0 >= dataTree.Count)
            {
                mUiState = Constants.UiState.Idle;
                ShowState();
                showTipMessage((string)FindResource("load_failed"));
                return;
            }

            mDataTree = dataTree;
            mUiState = Constants.UiState.DataLoaded;
            this.RomVersionTree.ItemsSource = mDataTree;
            ShowState();
        }

        private void notifySavingFinshed(bool success)
        {
            // 执行结束
            mSaveCts = null;
            mSaveTask = null;

            mUiState = Constants.UiState.DataLoaded;
            ShowState();

            if (!success)
            {
                showTipMessage((string)FindResource("save_failed"));
                return;
            }

            showTipMessage((string)FindResource("save_success"));
        }

        private VersionNodeItem FindNodeInTree(List<VersionNodeItem> list, string nodeName)
        {
            VersionNodeItem matchedNode = null;
            foreach (VersionNodeItem node in list)
            {
                if (null != node)
                {
                    if (0 == string.Compare(node.nodeName, nodeName))
                    {
                        return node;
                    }

                    matchedNode = FindNodeInTree(node.child, nodeName);
                    if (null != matchedNode)
                    {
                        return matchedNode;
                    }
                }
            }

            return null;
        }

        private void NodeName_Modified(object sender, RoutedEventArgs e)
        {
            VersionNodeItem curNode = FindNodeInTree(mDataTree, ((System.Windows.Controls.TextBox)sender).Text);
            if (null == curNode)
            {
                return;
            }

            if (!mDataT.IsValidNodeName(curNode))
            {
                showTipMessage((string)FindResource("current_node_name_is_invalid"));
                mDataT.ResetNodeName(curNode);
                ((System.Windows.Controls.TextBox)sender).Text = curNode.nodeName;
                return;
            }

            mDataT.ModifiedNodeName(curNode);
            if (curNode.modified)
            {
                ((System.Windows.Controls.TextBox)sender).Foreground 
                    = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (null != mLoadTask || null != mLoadCts) 
            {
                showTipMessage((string)FindResource("load_task_is_running_now"));
                return;
            }

            // 获取路径
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = (string)FindResource("please_select_the_rom_version_path");
            if (System.Windows.Forms.DialogResult.OK != dialog.ShowDialog())
            {
                return;
            }

            // 改变状态
            mUiState = Constants.UiState.DataLoading;
            ShowState();

            // 执行任务
            mLoadCts = new CancellationTokenSource();
            mLoadTask = new Task(() => DoLoading(mLoadCts.Token, dialog.SelectedPath), mLoadCts.Token);
            mLoadTask.Start();
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (null != mLoadCts)
            {
                // 停止加载任务
                mLoadCts.Cancel();
                mLoadCts = null;
                mLoadTask = null;
            }

            if (null != mSaveCts)
            {
                // 停止保存任务
                mSaveCts.Cancel();
                mSaveCts = null;
                mSaveTask = null;
            }

            this.RomVersionTree.ItemsSource = null;
            mUiState = Constants.UiState.Idle;
            ShowState();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (null != mSaveTask || null != mSaveCts)
            {
                showTipMessage((string)FindResource("save_task_is_running_now"));
                return;
            }

            // 改变状态
            mUiState = Constants.UiState.DataSaving;
            ShowState();

            // 执行任务
            mSaveCts = new CancellationTokenSource();
            mSaveTask = new Task(() => DoSaving(mSaveCts.Token, mDataTree), mSaveCts.Token);
            mSaveTask.Start();
        }

        /* Async Task */
        private void DoLoading(CancellationToken ct, string strFolderPath)
        {
            List<VersionNodeItem> dataTree = null;
            bool success = false;
            if (null == ct || string.IsNullOrWhiteSpace(strFolderPath))
            {
                goto safe_exit;
            }

            dataTree = mDataT.LoadDatas(ct, this, strFolderPath);
            if (null == dataTree || 0 >= dataTree.Count)
            {
                goto safe_exit;
            }

            mStrFolderPath = strFolderPath;
            success = true;
        safe_exit:
            this.Dispatcher.BeginInvoke(new Action(()
                => notifyLoadingFinshed(success, dataTree)));
        }

        private void DoSaving(CancellationToken ct, List<VersionNodeItem> dataTree)
        {
            bool success = mDataT.SaveDatas(ct, this, dataTree);

            this.Dispatcher.BeginInvoke(new Action(()
                => notifySavingFinshed(success)));

            // 保存完成后重新加载一下列表
            DoLoading(ct, mStrFolderPath);
        }
    }

    internal class VersionNodeItem 
    {
        /************************************************************************/
        /* 显示区域                                                             */
        /************************************************************************/

        // 树节点图标
        public string icon { get; set; }

        // 树节点名称, 若为中间节点, 则标识机型等信息, 若为叶子节点, 则标识版本号(带x)信息
        public string nodeName { get; set; }

        // 树节点描述, nodeName的信息补充
        public string desc { get; set; }

        // 树节点是否可编辑
        public bool editable { get; set; }

        /************************************************************************/
        /* 标记区域                                                             */
        /************************************************************************/
        // 节点是否被编辑过
        public bool modified { get; set; }

        /************************************************************************/
        /* 数据区域                                                             */
        /************************************************************************/
        public string modName { get; set; }
        public string company { get; set; }
        public string oem { get; set; }
        public string version { get; set; }
        public int[] versionI { get; set; }

        // 对应文件
        public FileInfo fileInfo { get; set; }

        // 子节点
        public List<VersionNodeItem> child { get; set; }

        // 构造函数, 初始化
        public VersionNodeItem()
        {
            this.icon = null;
            this.nodeName = null;
            this.desc = null;
            this.editable = false;
            this.modified = false;
            this.modName = null;
            this.company = null;
            this.version = null;
            this.versionI = null;
            this.fileInfo = null;
            this.child = null;
        }
    }
}
