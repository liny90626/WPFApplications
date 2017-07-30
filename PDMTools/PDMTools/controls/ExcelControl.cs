using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PDMTools.datas;
using PDMTools.defined;
using Spire.Xls;

namespace PDMTools.controls
{
    class ExcelControl
    {
        private MainWindow mWin;

        public ExcelControl(MainWindow win)
        {
            mWin = win;
        }

        public List<Operate> loadTemplateParams(Operate op)
        {
            if (null == op || Defined.OperateType.LoadTempalteParams != op.type)
            {
                return null;
            }

            Workbook excel = new Workbook();
            try
            {
                excel.LoadFromFile(getParamsFileName(op.value, ".xls"));
            }
            catch (Exception)
            {
                try
                {
                    excel.LoadFromFile(getParamsFileName(op.value, ".xlsx"));
                }
                catch (Exception)
                {
                    return null;
                }
            }

            List<Operate> paramsList = null;
            try
            {
                foreach (Worksheet sheet in excel.Worksheets)
                {
                    if (sheet.Name.Equals(
                        mWin.FindResource("template_params_sheet_name")))
                    {
                        int keyColIndex = -1, valueColIndex = -1, curIndex = 0;
                        foreach (CellRange column in sheet.Rows[0])
                        {
                            // 遍历第一行的每一列
                            if (column.Text.Equals(
                                mWin.FindResource("template_params_row_name_key")))
                            {
                                keyColIndex = curIndex;
                            }
                            else if (column.Text.Equals(
                                mWin.FindResource("template_params_row_name_value")))
                            {
                                valueColIndex = curIndex;
                            }

                            if (keyColIndex >= 0 && valueColIndex >= 0)
                            {
                                break;
                            }

                            ++curIndex;
                        }

                        if (keyColIndex < 0 || valueColIndex < 0)
                        {
                            return null;
                        }

                        paramsList = new List<Operate>();
                        Operate curOp = null;
                        bool skipFirst = false;
                        foreach (CellRange row in sheet.Rows)
                        {
                            if (!skipFirst)
                            {
                                skipFirst = true;
                                continue;
                            }

                            // 遍历每一行, 获取键值对
                            if (row.Columns[keyColIndex].Text.Equals("#"))
                            {
                                // 遇#表示跳过
                                continue;
                            }

                            curOp = new Operate();
                            curOp.type = Defined.OperateType.ReplaceWord;
                            curOp.key = row.Columns[keyColIndex].Text;
                            curOp.value = row.Columns[valueColIndex].Text;
                            paramsList.Add(curOp);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return paramsList;
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

        private string getParamsFileName(string templateRoot, string extension)
        {
            return templateRoot + Path.DirectorySeparatorChar 
                + mWin.FindResource("template_params_file_name")
                + extension;
        }

        private int _doReplaceAndGenerate(string dstFile, string srcFile, List<Operate> paramsList)
        {
            try
            {
                Workbook excel = new Workbook();
                excel.LoadFromFile(srcFile);
                foreach (Operate param in paramsList)
                {
                    if (Defined.OperateType.ReplaceWord == param.type)
                    {
                        try
                        {
                            excel.Replace("${" + param.key + "}", param.value);
                        }
                        catch (Exception)
                        {
                            // 替换动作可能会因找不到key而触发Exception
                        }
                    }
                }
                excel.SaveToFile(dstFile);
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }
    }
}
