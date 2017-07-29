using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using PDMTools.defined;
using PDMTools.datas;

namespace PDMTools.models
{
    /*
     * 管理工具发行文件相关数据及对应UI显示
     */
    public class ToolModel : BaseModel
    {
        private CheckBox mPublishToolCb = null;
        private Label mToolLabel = null;
        private Button mToolBtn = null;

        public override void init(MainWindow win)
        {
            base.init(win);
            mPublishToolCb = win.PublishTool;
            mToolLabel = win.SelectToolFileLabel;
            mToolBtn = win.SelectToolFileBtn;
            showState(Defined.UiState.Idle);  
        }

        public override void deinit()
        {
            mToolBtn = null;
            mToolLabel = null;
            mPublishToolCb = null;
            base.deinit();
        }

        public override void showState(Defined.UiState state)
        {
            if (!isInited())
            {
                return;
            }

            switch (state)
            {
                case Defined.UiState.SelectedTool:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        mPublishToolCb.IsEnabled = true;
                        mToolBtn.IsEnabled = true;
                    }
                    break;

                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedFirmware:
                    {
                        mPublishToolCb.IsEnabled = true;
                        mToolBtn.IsEnabled = false;
                    }
                    break;

                case Defined.UiState.Doing:
                    {
                        mPublishToolCb.IsEnabled = false;
                        mToolBtn.IsEnabled = false;
                    }
                    break;

                case Defined.UiState.Idle:
                default:
                    {
                        mToolLabel.IsEnabled = true;
                        mToolLabel.Content = mWin.FindResource("please_select_tool_file");

                        mToolBtn.IsEnabled = false;

                        mPublishToolCb.IsEnabled = false;
                        mPublishToolCb.IsChecked = false;
                    }
                    break;
            }
        }

        public override bool isValid(Defined.UiState state)
        {
            if (!isInited())
            {
                return false;
            }

            switch (state)
            {
                case Defined.UiState.SelectedTool:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        if (isValidToolFile(mToolLabel.Content.ToString()))
                        {
                            return true;
                        }
                    }
                    break;

                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.Doing:
                case Defined.UiState.Idle:
                default:
                    break;
            }

            return false;        
        }

        public override List<Operate> getOperates()
        {
            if (!isInited())
            {
                return null;
            }

            // 只生成列表, 不具体计算, 避免主线程开销
            List<Operate> list = new List<Operate>();

            // Tool file
            Operate curOp = new Operate();
            curOp.type = Defined.OperateType.CheckItem;
            curOp.key = Defined.KeyName.ToolFileName.ToString();
            curOp.value = mToolLabel.Content.ToString();

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileVersion;
            curOp.key = Defined.KeyName.ToolVersion.ToString();
            curOp.value = mToolLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileMd5;
            curOp.key = Defined.KeyName.ToolFileMd5.ToString();
            curOp.value = mToolLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileModifiedTime;
            curOp.key = Defined.KeyName.ToolFileModifiedTime.ToString();
            curOp.value = mToolLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileSizeByBytes;
            curOp.key = Defined.KeyName.ToolFileSize.ToString();
            curOp.value = mToolLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileSizeByM;
            curOp.key = Defined.KeyName.ToolFileSizeByManual.ToString();
            curOp.value = mToolLabel.Content.ToString();
            list.Add(curOp);

            return list;
        }

        public bool isNeedPublish()
        {
            if (!isInited())
            {
                return false;
            }

            return (bool)mPublishToolCb.IsChecked;
        }

        public void setSelectedToolFile(string selectedfile)
        {
            if (!isInited())
            {
                return;
            }

            mToolLabel.Content = selectedfile;
        }

        public void resetSelectedToolFile()
        {
            if (!isInited())
            {
                return;
            }

            mToolLabel.Content = mWin.FindResource("please_select_tool_file");
        }

        public bool isValidToolFile(string file)
        {
            if (!isInited())
            {
                return false;
            }

            if (!System.IO.File.Exists(file))
            {
                return false;
            }

            string filename = System.IO.Path.GetFileNameWithoutExtension(file);
            string extension = System.IO.Path.GetExtension(file);
            if (!extension.Equals(".zip") && !extension.Equals(".rar"))
            {
                return false;
            }

            Regex rgx = new Regex(@"[\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}");
            return rgx.IsMatch(filename);
        }
    }
}
