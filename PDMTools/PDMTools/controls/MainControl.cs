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

            mLogM.print((string)mWin.FindResource("start_generate_task_success"));
            mLogM.print((string)mWin.FindResource("start_split"));
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
                mLogM.print((string)mWin.FindResource("end_split"));
                mLogM.print((string)mWin.FindResource("stop_generate_task_success"));
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
            List<Operate> paramsList = loadParamsList(ct, operateList);
            if (null == paramsList)
            {
                completed();
                return;
            }

            List<Operate> outputsList = loadOutputsList(ct, operateList);
            if (null == outputsList)
            {
                completed();
                return;
            }

            // print current list
            printList(ct, paramsList);
            printList(ct, outputsList);

            // run list
            runList(ct, operateList);

            // notify ui
            completed();  
        }

        private List<Operate> loadParamsList(CancellationToken ct, List<Operate> operateList)
        {
            mLogM.print((string)mWin.FindResource("start_load_params_list"));

            // 从excel文件中导入的PDM参数信息列表
            List<Operate> paramsList = new List<Operate>();
            // 从所选文件中获取的信息进行二次校验的列表
            List<Operate> checkList = new List<Operate>();
            Operate newOp = null;
            foreach (Operate op in operateList)
            {
                ct.ThrowIfCancellationRequested();

                switch (op.type)
                {
                    case Defined.OperateType.LoadTempalteParams:
                        {
                            mLogM.print((string)mWin.FindResource("loading_template_params"));
                            paramsList = paramsList.Union(
                                mExcelC.loadTemplateParams(op)).ToList<Operate>();
                        }
                        break;

                    case Defined.OperateType.CalcFileVersion:
                        {
                            mLogM.print((string)mWin.FindResource("calc_file_version"));
                            newOp = mFileC.calcFileVersion(op);
                            if (null == newOp)
                            {
                                mLogM.print((string)mWin.FindResource("calc_file_version_failed"));
                                return null;
                            }
                            checkList.Add(newOp);
                        }
                        break;

                    case Defined.OperateType.CalcFileMd5:
                        {
                            mLogM.print((string)mWin.FindResource("calc_file_md5"));
                            newOp = mFileC.calcFileMd5(op);
                            if (null == newOp)
                            {
                                mLogM.print((string)mWin.FindResource("calc_file_md5_failed"));
                                return null;
                            }
                            paramsList.Add(newOp);
                        }
                        break;

                    case Defined.OperateType.CalcFileModifiedTime:
                        {
                            mLogM.print((string)mWin.FindResource("calc_file_modified_time"));
                            newOp = mFileC.calcFileModifiedTime(op);
                            if (null == newOp)
                            {
                                mLogM.print((string)mWin.FindResource("calc_file_modified_time_failed"));
                                return null;
                            }
                            paramsList.Add(newOp);
                        }
                        break;

                    case Defined.OperateType.CalcFileSizeByBytes:
                        {
                            mLogM.print((string)mWin.FindResource("calc_file_size"));
                            newOp = mFileC.calcFileSizeBytes(op);
                            if (null == newOp)
                            {
                                mLogM.print((string)mWin.FindResource("calc_file_size_failed"));
                                return null;
                            }
                            paramsList.Add(newOp);
                        }
                        break;

                    case Defined.OperateType.CalcFileSizeByM:
                        {
                            mLogM.print((string)mWin.FindResource("calc_file_size"));
                            newOp = mFileC.calcFileSizeByM(op);
                            if (null == newOp)
                            {
                                mLogM.print((string)mWin.FindResource("calc_file_size_failed"));
                                return null;
                            }
                            paramsList.Add(newOp);
                        }
                        break;

                    case Defined.OperateType.CheckItem:
                        checkList.Add(op);
                        break;

                    case Defined.OperateType.ReplaceWord:
                        paramsList.Add(op);
                        break;

                    case Defined.OperateType.OutputFile:
                    default:
                        break;
                }
            }

            if (!checkParamsList(paramsList, checkList))
            {
                mLogM.print((string)mWin.FindResource("check_params_list_failed"));
                return null;
            }

            return paramsList;
        }

        private bool checkParamsList(List<Operate> paramsList, List<Operate> checkList)
        {
            if (null == paramsList || null == checkList)
            {
                return false;
            }

            foreach (Operate checkOp in checkList)
            { 
                // 校验列表时不再判断操作类型, 主要校验键值对, 并且大小写不敏感
                foreach (Operate paramOp in paramsList)
                {
                    if (checkOp.key.Equals(paramOp.key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (checkOp.value.Equals(paramOp.value, StringComparison.CurrentCultureIgnoreCase))
                        {
                            break;
                        }
                        mLogM.print(paramOp.key + "(" + paramOp.value + ")" + " != "
                            + checkOp.key + "(" + checkOp.value + ")");
                        return false;
                    }
                }
            }

            return true;
        }

        private List<Operate> loadOutputsList(CancellationToken ct, List<Operate> operateList)
        {
            mLogM.print((string)mWin.FindResource("start_load_outputs_list"));

            // 输出文件列表
            List<Operate> outputsList = new List<Operate>();
            foreach (Operate op in operateList)
            {
                ct.ThrowIfCancellationRequested();

                switch (op.type)
                {
                    case Defined.OperateType.OutputFile:
                        outputsList.Add(op);
                        break;

                    default:
                        break;
                }
            }

            return outputsList;
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
