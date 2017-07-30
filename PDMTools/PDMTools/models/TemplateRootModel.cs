using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
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
                        if (isValidTemplateRoot(mTemplateRootLabel.Content.ToString()))
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

        public override List<Operate> getOperates(Defined.UiState state)
        {
            if (!isInited())
            {
                return null;
            }

            if (Defined.UiState.SelectedFirmware != state
                && Defined.UiState.SelectedTool != state
                && Defined.UiState.SelectedFirmwareAndTool != state)
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

            List<FileInfo> fileList = getAllFiles(
                mTemplateRootLabel.Content.ToString(), 0);
            foreach (FileInfo file in fileList)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(file.FullName).Equals(
                                   mWin.FindResource("template_params_file_name")))
                {
                    // 跳过PDM参数文件
                    continue;
                }

                if (file.DirectoryName.Contains((string)mWin.FindResource("output_folder_firmware")))
                {
                    if (Defined.UiState.SelectedFirmware == state
                        || Defined.UiState.SelectedFirmwareAndTool == state)
                    {
                        curOp = new Operate();
                        curOp.type = Defined.OperateType.InputFile;
                        curOp.key = Defined.KeyName.TemplateFirmwareFile.ToString();
                        curOp.value = file.FullName;
                        list.Add(curOp);
                    }
                }
                else if (file.DirectoryName.Contains((string)mWin.FindResource("output_folder_tool")))
                {
                    if (Defined.UiState.SelectedTool == state
                        || Defined.UiState.SelectedFirmwareAndTool == state)
                    {
                        curOp = new Operate();
                        curOp.type = Defined.OperateType.InputFile;
                        curOp.key = Defined.KeyName.TemplateToolFile.ToString();
                        curOp.value = file.FullName;
                        list.Add(curOp);
                    }
                }
                else
                {
                    curOp = new Operate();
                    curOp.type = Defined.OperateType.InputFile;
                    curOp.key = Defined.KeyName.TemplateRootFile.ToString();
                    curOp.value = file.FullName;
                    list.Add(curOp);
                }
            }
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

        private bool isValidTemplateRoot(string pathRoot)
        {
            // 检查根目录下是否具备PDM参数文件
            DirectoryInfo folders = new DirectoryInfo(pathRoot);
            bool hasParamsFile = false;
            foreach (FileInfo file in folders.GetFiles())
            {
                if (System.IO.Path.GetFileNameWithoutExtension(file.FullName).Equals(
                    mWin.FindResource("template_params_file_name")))
                {
                    hasParamsFile = true;
                    break;
                }
            }

            // 检查文件数量是否满足目标
            bool hasValidFileNumber = true;
            List<FileInfo> fileList = getAllFiles(pathRoot, 0);
            if (null == fileList ||
                fileList.Count < Defined.SupportFileMinNumber ||
                fileList.Count > Defined.SupportFileMaxNumber)
            {
                hasValidFileNumber = false;
            }

            return (hasParamsFile && hasValidFileNumber);
        }

        private List<FileInfo> getAllFiles(string pathRoot, int depth) 
        {
            if (null == pathRoot || depth > Defined.ScanDirectoriesMaxDepth)
            {
                return null;
            }

            List<FileInfo> fileList = new List<FileInfo>();
            DirectoryInfo folders = new DirectoryInfo(pathRoot);

            try
            {
                // 扫描目录中的子文件夹
                foreach (DirectoryInfo folder in folders.GetDirectories())
                {
                    fileList = fileList.Union(
                        getAllFiles(folder.FullName, ++depth)).ToList<FileInfo>();
                }

                // 扫描目录中的文件
                int scanFileCnt = 0;
                foreach (FileInfo file in folders.GetFiles())
                {
                    if (".xls".Equals(System.IO.Path.GetExtension(file.FullName)) ||
                        ".xlsx".Equals(System.IO.Path.GetExtension(file.FullName)) ||
                        ".doc".Equals(System.IO.Path.GetExtension(file.FullName)) ||
                        ".docx".Equals(System.IO.Path.GetExtension(file.FullName)))
                    {
                        fileList.Add(file);
                        ++scanFileCnt;
                        if (scanFileCnt > Defined.ScanDirectoryFileMaxNumber)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return fileList;
        }
    }
}
