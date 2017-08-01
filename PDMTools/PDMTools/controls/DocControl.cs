using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PDMTools.datas;
using PDMTools.defined;
using Spire.Doc;
using Spire.Doc.Collections;

namespace PDMTools.controls
{
    class DocControl
    {
        private MainWindow mWin;

        public DocControl(MainWindow win)
        {
            mWin = win;
        }

        public int doReplaceAndGenerate(string dstFolder, Operate inputOp, List<Operate> paramsList)
        {
            if (null == dstFolder || null == paramsList ||
                null == inputOp || inputOp.type != Defined.OperateType.InputFile)
            {
                return -1;
            }

            try
            {
                string dstFile = null;
                if (Defined.KeyName.TemplateFirmwareFile.ToString().Equals(inputOp.key))
                {
                    dstFile = dstFolder + Path.DirectorySeparatorChar
                        + mWin.FindResource("output_folder_firmware")
                        + Path.DirectorySeparatorChar
                        + System.IO.Path.GetFileName(inputOp.value);

                }
                else if (Defined.KeyName.TemplateToolFile.ToString().Equals(inputOp.key))
                {
                    dstFile = dstFolder + Path.DirectorySeparatorChar
                        + mWin.FindResource("output_folder_tool")
                        + Path.DirectorySeparatorChar
                        + System.IO.Path.GetFileName(inputOp.value);
                }
                else if (Defined.KeyName.TemplateRootFile.ToString().Equals(inputOp.key))
                {
                    dstFile = dstFolder + Path.DirectorySeparatorChar
                        + System.IO.Path.GetFileName(inputOp.value);
                }
                else
                {
                    return -1;
                }

                if (0 != _doReplaceAndGenerate(dstFile, inputOp.value, paramsList))
                {
                    return -1;
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }

        private int _doReplaceAndGenerate(string dstFile, string srcFile, List<Operate> paramsList)
        {
            try
            {
                DirectoryInfo dstDir = new DirectoryInfo(System.IO.Path.GetDirectoryName(dstFile));
                if (!dstDir.Exists)
                {
                    dstDir.Create();
                }

                Document doc = new Document();
                doc.LoadFromFile(srcFile);
                foreach (Operate param in paramsList)
                {
                    if (Defined.OperateType.ReplaceWord == param.type)
                    {
                        try
                        {
                            doc.Replace("${" + param.key + "}", param.value, true, false);
                        }
                        catch (Exception)
                        {
                            // 替换动作可能会因找不到key而触发Exception
                        }
                    }
                }

                doc.SaveToFile(dstFile);
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }
    }
}
