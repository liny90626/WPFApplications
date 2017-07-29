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
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PDMTools.controls;
using PDMTools.models;
using PDMTools.defined;
using PDMTools.datas;

namespace PDMTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainControl mMainC = null;

        private LogModel mLogM = null;
        private TemplateRootModel mTemplateRootM = null;
        private FirmwareModel mFirmwareM = null;
        private ToolModel mToolM = null;
        private ActionBarModel mActionBarM = null;

        private Defined.UiState mUiState = Defined.UiState.Idle;
        private Defined.UiState mLastUiState = Defined.UiState.Idle;

        public MainWindow()
        {
            InitializeComponent();

            // 版本号, [大版本].[小版本].[与2000/1/1的差值天数].[当天时间刻度]
            this.VersionLabel.Content = (string)FindResource("version") + ": "
                + App.ResourceAssembly.GetName(false).Version;

            initControls();
            initModels();
        }

        ~MainWindow()
        {
            deinitModels();
            deinitControls();
        }

        /*
         * 外部可操作函数 ---------------------------------------------------------
         */
        public void taskCompleted()
        {
            // 停止
            mUiState = mLastUiState;
            mLastUiState = Defined.UiState.Doing;
            showCurState(mUiState);
            mMainC.stopGenerate();
        }


        /*
         * 初始化函数 -------------------------------------------------------------
         */
        private void initControls() 
        {
            if (null == mMainC)
            {
                mMainC = new MainControl();
                mMainC.init(this);
            }
        }

        private void deinitControls()
        {
            if (null != mMainC)
            {
                mMainC.deinit();
                mMainC = null;
            }
        }

        private void initModels()
        {
            if (null == mLogM)
            {
                mLogM = new LogModel();
                mLogM.init(this);
            }

            if (null == mTemplateRootM)
            {
                mTemplateRootM = new TemplateRootModel();
                mTemplateRootM.init(this);
            }

            if (null == mFirmwareM)
            {
                mFirmwareM = new FirmwareModel();
                mFirmwareM.init(this);
            }

            if (null == mToolM)
            {
                mToolM = new ToolModel();
                mToolM.init(this);
            }

            if (null == mActionBarM)
            {
                mActionBarM = new ActionBarModel();
                mActionBarM.init(this);
            }
        }

        private void deinitModels()
        {
            if (null != mActionBarM)
            {
                mActionBarM.deinit();
                mActionBarM = null;
            }

            if (null != mToolM)
            {
                mToolM.deinit();
                mToolM = null;
            }

            if (null != mFirmwareM)
            {
                mFirmwareM.deinit();
                mFirmwareM = null;
            }

            if (null != mTemplateRootM)
            {
                mTemplateRootM.deinit();
                mTemplateRootM = null;
            }

            if (null != mLogM)
            {
                mLogM.deinit();
                mLogM = null;
            }
        }

        /*
         * 界面通用操作函数 ---------------------------------------------------------
         */
        private void showTipMessage(string message)
        {
            this.ShowMessageAsync((string)FindResource("tips"), 
                message, MessageDialogStyle.Affirmative, 
                new MetroDialogSettings() { AffirmativeButtonText = "确定" });
        }

        private string showSelectPathDialog(string tips)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = tips;

            if (System.Windows.Forms.DialogResult.OK != dialog.ShowDialog())
            {
                return null;
            }

            if (string.IsNullOrEmpty(dialog.SelectedPath))
            {
                showTipMessage((string)FindResource("selected_path_can_not_be_empty"));
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

            if (string.IsNullOrEmpty(dialog.FileName))
            {
                showTipMessage((string)FindResource("selected_file_can_not_be_empty"));
                return null;
            }

            return dialog.FileName;
        }

        private void showCurState(Defined.UiState state)
        {
            mTemplateRootM.showState(state);
            mFirmwareM.showState(state);
            mToolM.showState(state);
            mLogM.showState(state);
            mActionBarM.showState(state);
        }

        private bool isAllValidBeforeStart(Defined.UiState state) {
            switch (state) {
                case Defined.UiState.SelectedFirmware:
                    {
                        if (mTemplateRootM.isValid(state) &&
                            mFirmwareM.isValid(state) &&
                            mLogM.isValid(state) &&
                            mActionBarM.isValid(state))
                        {
                            return true;
                        }
                    }
                    break;

                case Defined.UiState.SelectedTool:
                    {
                        if (mTemplateRootM.isValid(state) &&
                            mToolM.isValid(state) &&
                            mLogM.isValid(state) &&
                            mActionBarM.isValid(state))
                        {
                            return true;
                        }
                    }
                    break;

                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        if (mTemplateRootM.isValid(state) &&
                            mFirmwareM.isValid(state) &&
                            mToolM.isValid(state) &&
                            mLogM.isValid(state) &&
                            mActionBarM.isValid(state))
                        {
                            return true;
                        }
                    }
                    break;

                default:
                    break;
            }
            return false;
        }

        private List<Operate> buildOperateListFromUi()
        {
            List<Operate> list = mTemplateRootM.getOperates(); ;
            switch (mUiState)
            {
                case Defined.UiState.SelectedFirmware:
                    {
                        list = list.Union(mFirmwareM.getOperates()).ToList<Operate>();
                    }
                    break;

                case Defined.UiState.SelectedTool:
                    {
                        list = list.Union(mToolM.getOperates()).ToList<Operate>();
                    }
                    break;

                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        list = list.Union(mFirmwareM.getOperates()).ToList<Operate>();
                        list = list.Union(mToolM.getOperates()).ToList<Operate>();
                    }
                    break;

                default:
                    return null;
            }

            return list;
        }

        /*
         * 界面交互函数 -------------------------------------------------------------
         */
        private void SelectTemplateRootBtn_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = showSelectPathDialog(
                (string)FindResource("please_select_template_root"));
            if (null == selectedPath)
            {
                return;
            }

            mTemplateRootM.setSelectedPath(selectedPath);
            if (!mTemplateRootM.isValid(Defined.UiState.SelectedTemplate))
            {
                mTemplateRootM.resetSelectedPath();
                showTipMessage((string)FindResource("selected_path_is_invalid"));
                return;
            }

            mLastUiState = mUiState;
            mUiState = Defined.UiState.SelectedTemplate;
            showCurState(mUiState);
        }

        private void PublishFirmware_OnCheck(object sender, RoutedEventArgs e)
        {
            switch(mUiState)
            {
                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedFirmware:
                    {
                        mLastUiState = mUiState;
                        mUiState = mFirmwareM.isNeedPublish() ?
                            Defined.UiState.SelectedFirmware :
                            Defined.UiState.SelectedTemplate;
                    }
                    break;

                case Defined.UiState.SelectedTool:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        mLastUiState = mUiState;
                        mUiState = mFirmwareM.isNeedPublish() ?
                            Defined.UiState.SelectedFirmwareAndTool :
                            Defined.UiState.SelectedTool;
                    }
                    break;

                default:
                    break;
            }

            showCurState(mUiState);
        }

        private void SelectImgFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string selecteFile = showSelectFileDialog((string)FindResource("filter_img_file"));
            if (null == selecteFile)
            {
                return;
            }

            mFirmwareM.setSelectedImgFile(selecteFile);
            if (!mFirmwareM.isValidImgFile(selecteFile))
            {
                mFirmwareM.resetSelectedImgFile();
                showTipMessage((string)FindResource("selected_file_is_invalid"));
                return;
            }
        }

        private void SelectZipFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string selecteFile = showSelectFileDialog((string)FindResource("filter_zip_file"));
            if (null == selecteFile)
            {
                return;
            }

            mFirmwareM.setSelectedZipFile(selecteFile);
            if (!mFirmwareM.isValidZipFile(selecteFile))
            {
                mFirmwareM.resetSelectedZipFile();
                showTipMessage((string)FindResource("selected_file_is_invalid"));
                return;
            }
        }

        private void PublishTool_OnCheck(object sender, RoutedEventArgs e)
        {
            switch (mUiState)
            {
                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedTool:
                    {
                        mLastUiState = mUiState;
                        mUiState = mToolM.isNeedPublish() ?
                            Defined.UiState.SelectedTool :
                            Defined.UiState.SelectedTemplate;
                    }
                    break;

                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        mLastUiState = mUiState;
                        mUiState = mToolM.isNeedPublish() ?
                            Defined.UiState.SelectedFirmwareAndTool :
                            Defined.UiState.SelectedFirmware;
                    }
                    break;

                default:
                    break;
            }

            showCurState(mUiState);
        }

        private void SelectToolFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string selecteFile = showSelectFileDialog((string)FindResource("filter_tool_file"));
            if (null == selecteFile)
            {
                return;
            }

            mToolM.setSelectedToolFile(selecteFile);
            if (!mToolM.isValidToolFile(selecteFile))
            {
                mToolM.resetSelectedToolFile();
                showTipMessage((string)FindResource("selected_file_is_invalid"));
                return;
            }
        }

        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Defined.UiState.Doing == mUiState)
            {
                taskCompleted();
                return;
            }

            if (!isAllValidBeforeStart(mUiState))
            {
                showTipMessage((string)FindResource("can_not_start_because_of_some_invalid_inputs"));
                return;
            }

            List<Operate> list = buildOperateListFromUi();
            if (null == list)
            {
                showTipMessage((string)FindResource("can_not_start_because_of_build_list_failed"));
                return;
            }
            mLastUiState = mUiState;
            mUiState = Defined.UiState.Doing;
            showCurState(mUiState);

            mMainC.startGenerate(list, mLogM);
            return;
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            mUiState = Defined.UiState.Idle;
            showCurState(mUiState);
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            mLogM.clear();
            showCurState(mUiState);
        } 
    }
}
