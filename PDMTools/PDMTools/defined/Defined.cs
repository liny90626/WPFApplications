using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDMTools.defined
{
    public class Defined
    {
        public const int ScanDirectoryFileMaxNumber = 10;
        public const int ScanDirectoriesMaxDepth = 5;
        public const int SupportFileMinNumber = 3;
        public const int SupportFileMaxNumber = 10;

        public const string OutputFolderPath = "C:\\Users\\LinKy\\Desktop\\WPF_PDMTools\\outputs";

        public enum UiState 
        {
            Idle = 0,
            SelectedTemplate,
            SelectedFirmware,
            SelectedTool,
            SelectedFirmwareAndTool,
            Doing,
        }

        public enum OperateType
        {
            LoadTempalteParams = 0,
            CalcFileVersion,
            CalcFileMd5,
            CalcFileModifiedTime,
            CalcFileSizeByBytes,
            CalcFileSizeByMBs,
            CheckItem,
            ReplaceWord,
            InputFile,
        }

        /* 静态键值名称 */
        public enum KeyName
        {
            TemplateRoot = 0,
            TemplateFirmwareFile,
            TemplateToolFile,
            TemplateRootFile,
            ImgFileName,
            ImgFileMd5,
            ImgFileModifiedTime,
            ImgFileSizeByBytes,
            ImgFileSizeByMBs,
            ZipFileName,
            ZipFileMd5,
            ZipFileModifiedTime,
            ZipFileSizeByBytes,
            ZipFileSizeByMBs,
            SoftwareVersion,
            ToolFileName,
            ToolFileMd5,
            ToolFileModifiedTime,
            ToolFileSizeByBytes,
            ToolFileSizeByMBs,
            ToolVersion,
            OutputsFileList,
        }
    }
}
