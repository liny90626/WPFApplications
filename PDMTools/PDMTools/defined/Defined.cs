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
            CalcFileVersion = 0,
            CalcFileMd5,
            CalcFileModifiedTime,
            CalcFileSize,
            CalcFileSizeByManual,
            ReplaceWord,
        }

        /* 静态键值名称 */
        public enum KeyName
        {
            ImgFileVersion = 0,
            ImgFileMd5,
            ImgFileModifiedTime,
            ImgFileSize,
            ImgFileSizeByManual,
            ZipFileVersion,
            ZipFileMd5,
            ZipFileModifiedTime,
            ZipFileSize,
            ZipFileSizeByManual,
            ToolFileVersion,
            ToolFileMd5,
            ToolFileModifiedTime,
            ToolFileSize,
            ToolFileSizeByManual,
        }
    }
}
