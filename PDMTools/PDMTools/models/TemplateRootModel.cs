using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using PDMTools.defined;
using PDMTools.datas;

namespace PDMTools.models
{
    /*
     * 管理模板根目录相关数据及对应UI显示
     */
    public class TemplateRootModel : BaseModel
    {
        private Label mTemplateRootLabel = null;
        private Button mTemplateRootBtn = null;
        public override void init(MainWindow win)
        {
            base.init(win);
            mTemplateRootLabel = win.SelectTemplateRootLabel;
            mTemplateRootBtn = win.SelectTemplateRootBtn;
            showState(Defined.UiState.Idle);  
        }

        public override void deinit()
        {
            mTemplateRootBtn = null;
            mTemplateRootLabel = null;
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
                case Defined.UiState.Doing:
                    {
                        mTemplateRootBtn.IsEnabled = false;
                    }
                    break;

                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.SelectedFirmwareAndTool:
                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedTool:
                
                    {
                        mTemplateRootBtn.IsEnabled = true;
                    }
                    break;

                case Defined.UiState.Idle:
                default:
                    {
                        mTemplateRootLabel.IsEnabled = true;
                        mTemplateRootLabel.Content = mWin.FindResource("please_select_template_root");
                        mTemplateRootBtn.IsEnabled = true;
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
                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.SelectedTool:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        if (System.IO.Directory.Exists(
                            mTemplateRootLabel.Content.ToString()))
                        {
                            return true;
                        }
                    }
                    break;

                
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

            // Template Root
            Operate curOp = new Operate();
            curOp.type = Defined.OperateType.LoadTempalteParams;
            curOp.key = Defined.KeyName.TemplateRoot.ToString();
            curOp.value = mTemplateRootLabel.Content.ToString();
            list.Add(curOp);
            return list;
        }

        public void setSelectedPath(string selectedPath)
        {
            if (!isInited())
            {
                return;
            }

            mTemplateRootLabel.Content = selectedPath;
        }

        public void resetSelectedPath()
        {
            if (!isInited())
            {
                return;
            }

            mTemplateRootLabel.Content = mWin.FindResource("please_select_template_root");
        }
    }
}
