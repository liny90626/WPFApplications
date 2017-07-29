using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PDMTools.datas;
using PDMTools.models;
using PDMTools.defined;

namespace PDMTools.controls
{
    public class MainControl
    {
        private MainWindow mWin = null;
        
        private FileControl mFileC = null;
        private ExcelControl mExcelC = null;

        private LogModel mLogM = null;

        private Task mGenerateTask = null;
        private CancellationTokenSource mCts = null;

        private bool mIsInited = false;

        public void init(MainWindow win)
        {
            mWin = win;
            mFileC = new FileControl(win);
            mExcelC = new ExcelControl(win);
            mIsInited = true;
        }

        public void deinit()
        {
            mExcelC = null;
            mFileC = null;
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

            mLogM = logM;

            if (!isInited())
            {
                return -1;
            }

            if (null != mGenerateTask || null != mCts) 
            {
                mLogM.print((string)mWin.FindResource("start_generate_task_failed_task_repeated"));
                return -1;
            }

            mCts = new CancellationTokenSource();
            mGenerateTask = new Task(() => run(mCts.Token, operateList), mCts.Token);

            mLogM.print((string)mWin.FindResource("start_split"));
            mLogM.print((string)mWin.FindResource("start_generate_task_success"));
            mGenerateTask.Start();
            return 0;
        }

        public int stopGenerate()
        {
            if (!isInited())
            {
                return -1;
            }

            if (null == mCts) 
            {
                if (null != mLogM)
                {
                    mLogM.print((string)mWin.FindResource("stop_generate_task_failed"));
                }
                return -1;
            }

            mCts.Cancel();
            mCts = null;
            mGenerateTask = null;

            if (null != mLogM)
            {
                mLogM.print((string)mWin.FindResource("stop_generate_task_success"));
                mLogM.print((string)mWin.FindResource("end_split"));
                mLogM = null;
            }
            return 0;
        }

        private void completed()
        {
            mWin.Dispatcher.BeginInvoke(new Action(()
                => mWin.taskCompleted()));
        }

        private void run(CancellationToken ct, List<Operate> operateList)
        {
            // load list
            operateList = loadList(ct, operateList);

            // print current list
            printList(ct, operateList);

            // run list
            runList(ct, operateList);

            // notify ui
            completed();  
        }

        private List<Operate> loadList(CancellationToken ct, List<Operate> operateList)
        {
            List<Operate> newList = new List<Operate>();
            Operate newOp = null;
            foreach (Operate op in operateList)
            {
                ct.ThrowIfCancellationRequested();

                if (null == op)
                {
                    continue;
                }

                switch (op.type)
                {
                    case Defined.OperateType.LoadTempalteParams:
                        {
                            newList = newList.Union(
                                mExcelC.loadTemplateParams(op)).ToList<Operate>();
                        }
                        break;

                    case Defined.OperateType.CalcFileVersion:
                        {
                            newOp = mFileC.calcFileVersion(op);
                            if (null != newOp)
                            {
                                newList.Add(newOp);
                            }
                        }
                        break;

                    case Defined.OperateType.CalcFileMd5:
                        {
                            newOp = mFileC.calcFileMd5(op);
                            if (null != newOp)
                            {
                                newList.Add(newOp);
                            }
                        }
                        break;

                    case Defined.OperateType.CalcFileModifiedTime:
                        {
                            newOp = mFileC.calcFileModifiedTime(op);
                            if (null != newOp)
                            {
                                newList.Add(newOp);
                            }
                        }
                        break;

                    case Defined.OperateType.CalcFileSizeByBytes:
                        {
                            newOp = mFileC.calcFileSizeBytes(op);
                            if (null != newOp)
                            {
                                newList.Add(newOp);
                            }
                        }
                        break;

                    case Defined.OperateType.CalcFileSizeByM:
                        {
                            newOp = mFileC.calcFileSizeByM(op);
                            if (null != newOp)
                            {
                                newList.Add(newOp);
                            }
                        }
                        break;

                    case Defined.OperateType.ReplaceWord:
                        newList.Add(op);
                        break;

                    default:
                        break;
                }
            }

            return newList;
        }

        private void printList(CancellationToken ct, List<Operate> operateList)
        {
            foreach (Operate op in operateList)
            {
                ct.ThrowIfCancellationRequested();

                if (null == op)
                {
                    continue;
                }

                mLogM.print(string.Format("{0}: {1} => {2}", 
                    op.type, op.key, op.value));
            }
        }

        private void runList(CancellationToken ct, List<Operate> operateList)
        {

        }
    }
}
