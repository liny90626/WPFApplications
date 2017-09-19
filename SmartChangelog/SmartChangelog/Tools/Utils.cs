using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SmartChangelog.Tools
{
    class Utils
    {
        private static bool mIsSvnBranchListLoading = false;

        public static void DoSvnBranchListLoad(Action action, int millisecond = 1000)
        {
            if (mIsSvnBranchListLoading)
            {
                // 确保请求间隔不会太短
                return;
            }

            mIsSvnBranchListLoading = true;
            new Action<Dispatcher, Action, int>(DoSvnBranchListLoadAsync).BeginInvoke(
                Dispatcher.CurrentDispatcher, 
                action, millisecond, null, null);
        }

        private static void DoSvnBranchListLoadAsync(Dispatcher dispatcher, Action action, int millisecond)
        {
            System.Threading.Thread.Sleep(millisecond);
            dispatcher.BeginInvoke(action);
            mIsSvnBranchListLoading = false;
        }
    }
}
