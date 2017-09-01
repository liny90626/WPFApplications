using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;
using PDMTools.datas;
using PDMTools.defined;

namespace PDMTools.controls
{
    public class FileControl
    {
        private MainWindow mWin;

        public FileControl(MainWindow win)
        {
            mWin = win;
        }

        public Operate calcFileVersion(Operate op)
        {
            if (null == op || Defined.OperateType.CalcFileVersion != op.type)
            {
                return null;
            }
            
            string filename = System.IO.Path.GetFileNameWithoutExtension(op.value);
            
            // 获取版本
            Regex rgxVersion = new Regex(@"[\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}");
            string version = rgxVersion.Match(filename).ToString();
            if (null == version)
            {
                return null;
            }

            // 获取机型
            Regex rgxDevice = new Regex(@"([a-zA-Z]{1,3}[\d]{1,3}[a-zA-Z]{1,3})-");
            string device = rgxDevice.Match(filename).ToString();
            if (!string.IsNullOrWhiteSpace(device))
            {
                version = "V" + version + "-" + device.Replace("-", "");
            }
            else
            { 
                // 文件名中允许不含有机型
                version = "V" + version;
            }

            Operate newOp = new Operate();
            newOp.type = Defined.OperateType.ReplaceWord;
            newOp.key = op.key;
            newOp.value = version;
            return newOp;
        }

        public Operate calcFileMd5(Operate op)
        {
            if (null == op || Defined.OperateType.CalcFileMd5 != op.type)
            {
                return null;
            }

            String md5 = null;
            try
            {
                FileStream file = new FileStream(op.value, FileMode.Open);
                MD5 md5Provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5Provider.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

                md5 = sb.ToString() ;
            }
            catch (Exception)
            {
                return null;
            }

            Operate newOp = new Operate();
            newOp.type = Defined.OperateType.ReplaceWord;
            newOp.key = op.key;
            newOp.value = md5.ToUpper();
            return newOp;
        }

        public Operate calcFileModifiedTime(Operate op)
        {
            if (null == op || Defined.OperateType.CalcFileModifiedTime != op.type)
            {
                return null;
            }

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(op.value);
            }
            catch (Exception)
            {
                return null;
            }

            if (null == fi)
            {
                return null;
            }

            Operate newOp = new Operate();
            newOp.type = Defined.OperateType.ReplaceWord;
            newOp.key = op.key;
            newOp.value = string.Format("{0:yyyy-MM-dd HH:mm}", fi.LastWriteTime);
            return newOp;
        }

        public Operate calcFileSizeBytes(Operate op)
        {
            if (null == op || Defined.OperateType.CalcFileSizeByBytes != op.type)
            {
                return null;
            }

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(op.value);
            }
            catch (Exception)
            {
                return null;
            }

            if (null == fi)
            {
                return null;
            }

            Operate newOp = new Operate();
            newOp.type = Defined.OperateType.ReplaceWord;
            newOp.key = op.key;
            newOp.value = string.Format("{0:N0} {1}", 
                fi.Length, mWin.FindResource("byte_unit"));
            return newOp;
        }

        public Operate calcFileSizeByM(Operate op)
        {
            if (null == op || Defined.OperateType.CalcFileSizeByMBs != op.type)
            {
                return null;
            }

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(op.value);
            }
            catch (Exception)
            {
                return null;
            }

            if (null == fi)
            {
                return null;
            }

            Operate newOp = new Operate();
            newOp.type = Defined.OperateType.ReplaceWord;
            newOp.key = op.key;
            newOp.value = string.Format("{0:N2} {1}",
                ((double)fi.Length)/1024/1024, mWin.FindResource("million_bytes_unit"));
            return newOp;
        }

        public void backupLastOutputs(string outputFolderPath)
        {
            if (System.IO.Directory.Exists(outputFolderPath))
            {
                DirectoryInfo folderBak = new DirectoryInfo(outputFolderPath);
                folderBak.MoveTo(backupDstFolderName(outputFolderPath));
            }

            Directory.CreateDirectory(outputFolderPath);
        }

        public void doToolFileCopy(string dstFolder, Operate inputOp)
        {
            string dstFile = dstFolder + Path.DirectorySeparatorChar
                    + mWin.FindResource("output_folder_tool")
                    + Path.DirectorySeparatorChar
                    + System.IO.Path.GetFileName(inputOp.value);

            FileInfo fileInfo = new FileInfo(inputOp.value);
            fileInfo.CopyTo(dstFile);
        }

        public bool isExcelFile(string file)
        {
            string extention = System.IO.Path.GetExtension(file);

            return ".xls".Equals(extention) || ".xlsx".Equals(extention);
        }

        public bool isWordFile(string file)
        {
            string extention = System.IO.Path.GetExtension(file);

            return ".doc".Equals(extention) || ".docx".Equals(extention);
        }

        private string backupDstFolderName(string outputFolderPath)
        {
            string parentFolder = System.IO.Directory.GetParent(outputFolderPath).FullName;
            string folderName = System.IO.Path.GetFileName(outputFolderPath) 
                + DateTime.Now.ToString("_bak_yyyy_MM_dd_hh_mm_ss");
            return parentFolder + Path.DirectorySeparatorChar + folderName;
        }
    }
}
