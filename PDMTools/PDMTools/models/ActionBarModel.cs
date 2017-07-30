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
     * 管理动作按钮及对应UI显示
     */
    public class ActionBarModel : BaseModel
    {
        private Button mRunBtn = null;
        private Button mResetBtn = null;
        private Button mClearBtn = null;

        public override void init(MainWindow win)
        {
            base.init(win);
            mRunBtn = win.RunBtn;
            mResetBtn = win.ResetBtn;
            mClearBtn = win.ClearBtn;
            showState(Defined.UiState.Idle);
        }

        public override void deinit()
        {
            mClearBtn = null;
            mResetBtn = null;
            mRunBtn = null;
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
                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedTool:
                case Defined.UiState.SelectedFirmwareAndTool: 
                    {
                        mRunBtn.IsEnabled = true;
                        mRunBtn.Content = (string)mWin.FindResource("start");

                        mResetBtn.IsEnabled = true;
                        mClearBtn.IsEnabled = true;
                    }
                    break;

                case Defined.UiState.Doing:
                    {
                        mRunBtn.IsEnabled = true;
                        mRunBtn.Content = (string)mWin.FindResource("stop");

                        mResetBtn.IsEnabled = false;
                        mClearBtn.IsEnabled = false;
                    }
                    break;

                case Defined.UiState.Idle:
                default:
                    {
                        mRunBtn.IsEnabled = false;
                        mRunBtn.Content = (string)mWin.FindResource("start");

                        mResetBtn.IsEnabled = false;
                        mClearBtn.IsEnabled = false;
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

            return true;
        }

        public override List<Operate> getOperates(Defined.UiState state)
        {
            return null;
        }

    }
}
