using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CfgConverter.utils
{
    class FileTools
    {
        public static bool isFileExist(System.Windows.Controls.Label label)
        {
            string fileStr = label.Content.ToString();
            if (string.IsNullOrWhiteSpace(fileStr))
            {
                return false;
            }

            FileInfo fileInfo = new FileInfo(fileStr);
            if (!fileInfo.Exists)
            {
                return false;
            }

            return true;
        }

        public static bool isFolderExist(System.Windows.Controls.Label label)
        {
            string folderStr = label.Content.ToString();
            if (string.IsNullOrWhiteSpace(folderStr))
            {
                return false;
            }

            DirectoryInfo folderInfo = new DirectoryInfo(folderStr);
            if (!folderInfo.Exists)
            {
                return false;
            }

            return true;
        }
    }
}
