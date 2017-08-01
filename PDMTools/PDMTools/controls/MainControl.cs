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
        private DocControl mDocC = null;

        private LogModel mLogM = null;

        private Task mGenerateTask = null;
        private CancellationTokenSource mCts = null;

        private bool mIsInited = false;

        public void init(MainWindow win)
        {
            mWin = win;
            mFileC = new FileControl(win);
            mExcelC = new ExcelControl(win);
            mDocC = new DocControl(win);
            mIsInited = true;
        }

        public void deinit()
        {
            mDocC = null;
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
            // 加载参数列表
            List<Operate> paramsList = loadParamsList(ct, operateList);
            if (null == paramsList)
            {
                completed();
                return;
            }

            // 加载输入文件列表
            List<Operate> inputsList = loadInputsList(ct, operateList);
            if (null == inputsList)
            {
                completed();
                return;
            }

            // 生成输出文件列表
            Operate outputFiles = loadOutputFiles(ct, inputsList, operateList);
            if (null == outputFiles)
            {
                completed();
                return;
            }
            paramsList.Add(outputFiles);

            // 打印列表信息
            printList(ct, paramsList);
            printList(ct, inputsList);

            // 执行列表
            runList(ct, paramsList, inputsList);

            // 通知UI
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
                            List<Operate> newList = mExcelC.loadTemplateParams(op);
                            if (null == newList)
                            {
                                mLogM.print((string)mWin.FindResource("loading_template_params_failed"));
                                return null;
                            }

                            paramsList = paramsList.Union(newList).ToList<Operate>();
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

                    case Defined.OperateType.CalcFileSizeByMBs:
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

                    case Defined.OperateType.InputFile:
                        break;

                    default:
                        break;
                }
            }

            if (!checkParamsList(ct, paramsList, checkList))
            {
                mLogM.print((string)mWin.FindResource("check_params_list_failed"));
                return null;
            }

            return paramsList;
        }

        private bool checkParamsList(CancellationToken ct, List<Operate> paramsList, List<Operate> checkList)
        {
            foreach (Operate checkOp in checkList)
            { 
                // 校验列表时不再判断操作类型, 主要校验键值对, 并且大小写不敏感
                foreach (Operate paramOp in paramsList)
                {
                    ct.ThrowIfCancellationRequested();

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

        private List<Operate> loadInputsList(CancellationToken ct, List<Operate> operateList)
        {
            mLogM.print((string)mWin.FindResource("start_load_inputs_list"));

            // 输出文件列表
            List<Operate> inputsList = new List<Operate>();
            foreach (Operate op in operateList)
            {
                ct.ThrowIfCancellationRequested();

                switch (op.type)
                {
                    case Defined.OperateType.InputFile:
                        inputsList.Add(op);
                        break;

                    default:
                        break;
                }
            }

            return inputsList;
        }

        private Operate loadOutputFiles(CancellationToken ct, List<Operate> inputsFileList, List<Operate> operateList)
        {
            mLogM.print((string)mWin.FindResource("start_load_output_files"));

            // 让工程文件在说明文件之前, 所以用operateList去合并inputsFileList
            List<Operate> list = inputsFileList.Union(operateList).ToList<Operate>();
            List<string> firmwareFiles = new List<string>();
            List<string> toolFiles = new List<string>();

            foreach (Operate op in list)
            {
                ct.ThrowIfCancellationRequested();
                
                if (Defined.OperateType.InputFile != op.type
                    && Defined.OperateType.CheckItem != op.type)
                {
                    continue;
                }

                string fileName = System.IO.Path.GetFileName(op.value);
                string extension = System.IO.Path.GetExtension(op.value);
                string fileType = null;
                if (".xls".Equals(extension) || ".xlsx".Equals(extension) ||
                    ".doc".Equals(extension) || ".docx".Equals(extension))
                {
                    // 文档类文件视为说明文件
                    fileType = (string)mWin.FindResource("explain_file");
                }
                else
                {
                    // 其他文件统一视为工程文件
                    fileType = (string)mWin.FindResource("engineering_file");
                }

                string line = fileName + " - " + fileType + " - " 
                    + mWin.FindResource("pdm_publish");;
                if (Defined.KeyName.TemplateFirmwareFile.ToString().Equals(op.key)
                    || Defined.KeyName.ImgFileName.ToString().Equals(op.key)
                    || Defined.KeyName.ZipFileName.ToString().Equals(op.key))
                {
                    firmwareFiles.Add(line);
                }
                else if (Defined.KeyName.TemplateToolFile.ToString().Equals(op.key)
                    || Defined.KeyName.ToolFileName.ToString().Equals(op.key))
                {
                    toolFiles.Add(line);
                }
            }

            string content = "";
            int fileTypeCnt = 0;
            if (0 < firmwareFiles.Count)
            {
                ++fileTypeCnt;
                content += (fileTypeCnt + "、" 
                    + mWin.FindResource("output_folder_firmware")
                    + Environment.NewLine);
                int fileCnt = 0;
                foreach (string line in firmwareFiles)
                {
                    ++fileCnt;
                    content += (fileCnt + "）"
                        + line + Environment.NewLine);
                }
            }

            if (0 < toolFiles.Count)
            {
                ++fileTypeCnt;
                content += (fileTypeCnt + "、"
                    + mWin.FindResource("output_folder_tool")
                    + Environment.NewLine);
                int fileCnt = 0;
                foreach (string line in toolFiles)
                {
                    ++fileCnt;
                    content += (fileCnt + "）"
                        + line + Environment.NewLine);
                }
            }

            Operate newOp = new Operate();
            newOp.type = Defined.OperateType.ReplaceWord;
            newOp.key = Defined.KeyName.OutputsFileList.ToString();
            newOp.value = content;
            return newOp;
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

        private void runList(CancellationToken ct, 
            List<Operate> paramsList, List<Operate> inputsList)
        {
            mLogM.print((string)mWin.FindResource("start_load_run_list"));

            string dstFolder = Defined.OutputFolderPath;
            // 备份上一次输出目录, 并确保输出目录存在
            mFileC.backupLastOutputs(dstFolder);

            foreach (Operate inputOp in inputsList)
            {
                ct.ThrowIfCancellationRequested();

                if (inputOp.type != Defined.OperateType.InputFile)
                {
                    continue;
                }

                if (mFileC.isExcelFile(inputOp.value))
                {
                    mLogM.print("#" + inputOp.value + "#"
                            + mWin.FindResource("do_replace_and_generate"));
                    if (0 == mExcelC.doReplaceAndGenerate(dstFolder, inputOp, paramsList))
                    {
                        mLogM.print("#" + inputOp.value + "#"
                            + mWin.FindResource("do_replace_and_generate_success"));
                    }
                    else
                    {
                        mLogM.print("#" + inputOp.value + "#"
                            + mWin.FindResource("do_replace_and_generate_failed"));
                    }
                }
                else if (mFileC.isWordFile(inputOp.value))
                {
                    mLogM.print("#" + inputOp.value + "#"
                            + mWin.FindResource("do_replace_and_generate"));
                    if (0 == mDocC.doReplaceAndGenerate(dstFolder, inputOp, paramsList))
                    {
                        mLogM.print("#" + inputOp.value + "#"
                            + mWin.FindResource("do_replace_and_generate_success"));
                    }
                    else
                    {
                        mLogM.print("#" + inputOp.value + "#"
                            + mWin.FindResource("do_replace_and_generate_failed"));
                    }
                }
            }
        }

        
    }
}
