using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDMTools.datas;
using PDMTools.models;
using System.Threading.Tasks;

namespace PDMTools.controls
{
    public class MainControl
    {
        private MainWindow mWin = null;
        private LogModel mLogM = null;

        private Task mGenerateTask = null;
        private List<Operate> mOperateList = null;

        private bool mIsInited = false;

        public void init(MainWindow win)
        {
            mWin = win;
            mIsInited = true;
        }

        public void deinit()
        {
            mWin = null;
            mIsInited = false;
        }

        public bool isInited() 
        {
            return mIsInited;
        }

        public int startGenerate(List<Operate> operateList, LogModel logM)
        {
            if (null == operateList || null == logM)
            {
                return -1;
            }

            mOperateList = operateList;
            mLogM = logM;

            if (!isInited())
            {
                mLogM.print((string)mWin.FindResource("control_is_not_inited"));
                return -1;
            }

            if (null != mGenerateTask) 
            {
                mLogM.print((string)mWin.FindResource("start_generate_task_failed_task_repeated"));
            }

            mLogM.print((string)mWin.FindResource("start_generate_task_failed_task_repeated"));
            return 0;
        }

        public int stopGenerate()
        {
            return -1;
        }


    }
}
