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
    public abstract class BaseModel
    {
        protected MainWindow mWin = null;
        protected bool mIsInited = false;

        public virtual void init(MainWindow win)
        {
            mWin = win;
            mIsInited = true;
        }
        public virtual void deinit()
        {
            mWin = null;
            mIsInited = false;
        }

        public virtual bool isInited()
        {
            return mIsInited;
        }

        public abstract void showState(Defined.UiState state);
        public abstract bool isValid(Defined.UiState state);
        public abstract List<Operate> getOperates(Defined.UiState state);
    }
}
