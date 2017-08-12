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
using System.Windows.Forms;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using CfgConverter.defineds;
using CfgConverter.utils;
using CfgConverter.controls;

namespace CfgConverter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Constant.UiState mUiState = Constant.UiState.Idle;
        private Constant.UiState mLastUiState = Constant.UiState.Idle;

        private MainControl mMainC = null;

        public MainWindow()
        {
            InitializeComponent();

            // 版本号, [大版本].[小版本].[与2000/1/1的差值天数].[当天时间刻度]
            this.VersionLabel.Content = (string)FindResource("version") + ": "
                + App.ResourceAssembly.GetName(false).Version;

            mMainC = new MainControl(this);

            reset();
        }

        ~MainWindow()
        {
            mMainC = null;
        }

        /*
         * Ui状态机相关
         */
        private void inputFileReady(bool ready)
        {
            switch (mUiState)
            {
                case Constant.UiState.Idle:
                    {
                        if (ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.InputFileReady;
                            showState();
                        }
                    }
                    break;

                case Constant.UiState.InputFileReady:
                    {
                        if (!ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.Idle;
                            showState();
                        }
                    }
                    break;

                case Constant.UiState.OutputFolderReady:
                    {
                        if (ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.AllReady;
                            showState();
                        }
                    }
                    break;

                case Constant.UiState.AllReady:
                    {
                        if (!ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.OutputFolderReady;
                            showState();
                        }
                    }
                    break;

                default:
                    // 非法状态, 状态机不响应
                    break;
            }
        }

        private void outputFolderReady(bool ready)
        {
            switch (mUiState)
            {
                case Constant.UiState.Idle:
                    {
                        if (ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.OutputFolderReady;
                            showState();
                        }
                    }
                    break;

                case Constant.UiState.OutputFolderReady:
                    {
                        if (!ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.Idle;
                            showState();
                        }
                    }
                    break;

                case Constant.UiState.InputFileReady:
                    {
                        if (ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.AllReady;
                            showState();
                        }
                    }
                    break;

                case Constant.UiState.AllReady:
                    {
                        if (!ready)
                        {
                            mLastUiState = mUiState;
                            mUiState = Constant.UiState.InputFileReady;
                            showState();
                        }
                    }
                    break;

                default:
                    // 非法状态, 状态机不响应
                    break;
            }
        }

        private void action()
        {
            switch (mUiState)
            {
                case Constant.UiState.Working:
                    {
                        // 停止任务, 先停再更新界面
                        mMainC.stopTask();

                        mUiState = mLastUiState;
                        mLastUiState = Constant.UiState.Working;
                        showState();
                    }
                    break;

                case Constant.UiState.AllReady:
                    {
                        if (!FileTools.isFileExist(this.SelectInputFileLabel)
                            || !FileTools.isFolderExist(this.SelectOutputFolderLabel))
                        {
                            showTipMessage((string)FindResource("please_select_valid_file_and_folder"));
                            return;
                        }

                        clear();    // 每次开始前清空上一次执行日志

                        // 开始任务, 先更新界面再开始
                        mLastUiState = mUiState;
                        mUiState = Constant.UiState.Working;
                        showState();

                        mMainC.startTask(this.SelectInputFileLabel.Content.ToString(),
                            this.SelectOutputFolderLabel.Content.ToString());
                    }
                    break;

                default:
                    {
                        // 非法状态, 状态机有误
                        showTipMessage((string)FindResource("state_error"));
                    }
                    break;
            }
        }

        private void reset()
        {
            switch (mUiState)
            {
                case Constant.UiState.Working:
                    {
                        // 非法状态, 状态机有误
                        showTipMessage((string)FindResource("state_error"));
                    }
                    break;

                default:
                    {
                        mLastUiState = mUiState;
                        mUiState = Constant.UiState.Idle;
                        showState();

                        // 清空日志
                        clear();
                    }
                    break;
            }
        }

        private void clear()
        {
            switch (mUiState)
            {
                case Constant.UiState.Working:
                    {
                        // 非法状态, 状态机有误
                        showTipMessage((string)FindResource("state_error"));
                    }
                    break;

                default:
                    {
                        // 只清空日志, 状态机无改变
                        this.LogTxt.Text = "";
                    }
                    break;
            }
        }

        private void showState()
        {
            showInputAreaState(mUiState);
            showOutputAreaState(mUiState);
            showLogAreaState(mUiState);
            showActionBarState(mUiState);
        }

        private void showInputAreaState(Constant.UiState state)
        {
            switch (state)
            {
                case Constant.UiState.Working:
                    {
                        this.SelectInputFileLabel.IsEnabled = false;
                        this.SelectInputFileBtn.IsEnabled = false;
                    }
                    break;

                case Constant.UiState.InputFileReady:
                case Constant.UiState.AllReady:
                    {
                        this.SelectInputFileLabel.IsEnabled = true;
                        this.SelectInputFileBtn.IsEnabled = true;
                        this.SelectInputFileBtn.Content =
                            FindResource("change");
                    }
                    break;

                default:
                    {
                        this.SelectInputFileLabel.IsEnabled = true;
                        this.SelectInputFileLabel.Content =
                            FindResource("please_select_your_input_file");
                        this.SelectInputFileBtn.IsEnabled = true;
                        this.SelectInputFileBtn.Content =
                            FindResource("browser");
                    }
                    break;
            }
        }

        private void showOutputAreaState(Constant.UiState state)
        {
            switch (state)
            {
                case Constant.UiState.Working:
                    {
                        this.SelectOutputFolderLabel.IsEnabled = false;
                        this.SelectOutputFolderBtn.IsEnabled = false;
                    }
                    break;

                case Constant.UiState.OutputFolderReady:
                case Constant.UiState.AllReady:
                    {
                        this.SelectOutputFolderLabel.IsEnabled = true;
                        this.SelectOutputFolderBtn.IsEnabled = true;
                        this.SelectOutputFolderBtn.Content =
                            FindResource("change");
                    }
                    break;

                default:
                    {
                        this.SelectOutputFolderLabel.IsEnabled = true;
                        this.SelectOutputFolderLabel.Content =
                            FindResource("please_select_your_output_folder");
                        this.SelectOutputFolderBtn.IsEnabled = true;
                        this.SelectOutputFolderBtn.Content =
                            FindResource("browser");
                    }
                    break;
            }
        }

        private void showLogAreaState(Constant.UiState state)
        {
            switch (state)
            {
                case Constant.UiState.Working:
                    {
                        this.LogTxt.IsEnabled = false;
                    }
                    break;

                default:
                    {
                        this.LogTxt.IsEnabled = true;
                    }
                    break;
            }
        }

        private void showActionBarState(Constant.UiState state)
        {
            switch (state)
            {
                case Constant.UiState.Working:
                    {
                        this.ActionBtn.IsEnabled = true;
                        this.ActionBtn.Content =
                            FindResource("stop");
                        this.ResetBtn.IsEnabled = false;
                        this.ClearBtn.IsEnabled = false;
                    }
                    break;

                case Constant.UiState.AllReady:
                    {
                        this.ActionBtn.IsEnabled = true;
                        this.ActionBtn.Content =
                            FindResource("start");
                        this.ResetBtn.IsEnabled = true;
                        this.ClearBtn.IsEnabled = true;
                    }
                    break;

                default:
                    {
                        this.ActionBtn.IsEnabled = false;
                        this.ActionBtn.Content =
                            FindResource("start");
                        this.ResetBtn.IsEnabled = true;
                        this.ClearBtn.IsEnabled = true;
                    }
                    break;
            }
        }

        /*
         * 通用操作函数
         */
        private string showSelectFolderDialog(string tips)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = tips;

            if (System.Windows.Forms.DialogResult.OK != dialog.ShowDialog())
            {
                return null;
            }

            return dialog.SelectedPath;
        }

        private string showSelectFileDialog(string filter)
        {
            OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;
            if (System.Windows.Forms.DialogResult.OK != dialog.ShowDialog())
            {
                return null;
            }

            return dialog.FileName;
        }

        private void setSmartLabelValue(System.Windows.Controls.Label label, string value)
        {
            label.Content = value;
        }

        /*
         * 界面交互函数
         */
        private void SelectInputFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileStr = showSelectFileDialog(
                (string)FindResource("filter_input_file"));
            if (string.IsNullOrWhiteSpace(fileStr))
            {
                setSmartLabelValue(this.SelectInputFileLabel, 
                    (string)FindResource("please_select_your_input_file"));
                showTipMessage((string)FindResource("selected_file_is_invalid"));

                inputFileReady(false);
                return;
            }

            setSmartLabelValue(this.SelectInputFileLabel, fileStr);
            inputFileReady(true);
        }

        private void SelectOutputFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            string folderStr = showSelectFolderDialog(
                (string)FindResource("please_select_your_output_folder"));
            if (string.IsNullOrWhiteSpace(folderStr))
            {
                setSmartLabelValue(this.SelectOutputFolderLabel, 
                    (string)FindResource("please_select_your_output_folder"));
                showTipMessage((string)FindResource("selected_folder_is_invalid"));

                outputFolderReady(false);
                return;
            }

            setSmartLabelValue(this.SelectOutputFolderLabel, folderStr);
            outputFolderReady(true);
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            action();
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            reset();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            clear();
        }

        /*
         * 外部接口函数
         */
        public void showTipMessage(string message)
        {
            this.ShowMessageAsync((string)FindResource("tips"),
                message, MessageDialogStyle.Affirmative,
                new MetroDialogSettings()
                {
                    AffirmativeButtonText =
                        (string)FindResource("ok")
                });
        }

        public void showLog(string line)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.LogTxt.Text += (DateTime.Now.ToString("[hh:mm:ss] ")
                        + line + Environment.NewLine);
                    this.LogTxt.ScrollToEnd();
                }));
        }

        public void notifyFinshed()
        {
            showLog((string)FindResource("task_is_finished"));
            mUiState = mLastUiState;
            mLastUiState = Constant.UiState.Working;
            showState();
        }
    }
}
