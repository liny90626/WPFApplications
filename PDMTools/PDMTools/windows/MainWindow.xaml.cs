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
using PDMTools.models;
using PDMTools.defined;

namespace PDMTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private LogModel mLogM = null;
        private TemplateRootModel mTemplateRootM = null;
        private FirmwareModel mFirmwareM = null;
        private ToolModel mToolM = null;
        private ActionBarModel mActionBarM = null;

        private Defined.UiState mUiState = Defined.UiState.Idle;

        public MainWindow()
        {
            InitializeComponent();

            // 版本号, [大版本].[小版本].[与2000/1/1的差值天数].[当天时间刻度]
            this.VersionLabel.Content = (string)FindResource("version") + ": "
                + App.ResourceAssembly.GetName(false).Version;

            initModels();
        }

        ~MainWindow()
        {
            deinitModels();
        }

        /*
         * 初始化函数 -------------------------------------------------------------
         */

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
            System.Windows.MessageBox.Show(this, message,
                    (string)FindResource("tips"));
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
                        mUiState = mFirmwareM.isNeedPublish() ?
                            Defined.UiState.SelectedFirmware :
                            Defined.UiState.SelectedTemplate;
                    }
                    break;

                case Defined.UiState.SelectedTool:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
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
                        mUiState = mToolM.isNeedPublish() ?
                            Defined.UiState.SelectedTool :
                            Defined.UiState.SelectedTemplate;
                    }
                    break;

                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
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
