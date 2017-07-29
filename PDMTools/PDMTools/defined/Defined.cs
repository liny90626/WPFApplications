using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDMTools.defined
{
    public class Defined
    {
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
            CalcFileSizeByM,
            CheckItem,
            ReplaceWord,
            OutputFile,
        }

        /* 静态键值名称 */
        public enum KeyName
        {
            TemplateRoot = 0,
            ImgFileName,
            ImgFileMd5,
            ImgFileModifiedTime,
            ImgFileSize,
            ImgFileSizeByManual,
            ZipFileName,
            ZipFileMd5,
            ZipFileModifiedTime,
            ZipFileSize,
            ZipFileSizeByManual,
            SoftwareVersion,
            ToolFileName,
            ToolFileMd5,
            ToolFileModifiedTime,
            ToolFileSize,
            ToolFileSizeByManual,
            ToolVersion,
        }
    }
}
