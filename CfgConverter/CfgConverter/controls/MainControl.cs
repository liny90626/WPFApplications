using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Spire.Xls;

using CfgConverter.defineds;
using System.IO;

namespace CfgConverter.controls
{
    class MainControl
    {
        private MainWindow mWin = null;

        private Task mGenerateTask = null;
        private CancellationTokenSource mCts = null;

        public MainControl(MainWindow win)
        {
            mWin = win;
        }

        ~MainControl()
        {
            mWin = null;
        }

        public int startTask(string fileStr, string folderStr)
        {
            if (null != mGenerateTask || null != mCts)
            {
                mWin.showLog((string)mWin.FindResource("task_has_been_started_already"));
                return -1;
            }

            mCts = new CancellationTokenSource();
            mGenerateTask = new Task(() => run(mCts.Token, fileStr, folderStr), mCts.Token);

            mWin.showLog((string)mWin.FindResource("start_task_success"));
            mGenerateTask.Start();
            return 0;
        }

        public int stopTask()
        {
            if (null == mCts)
            {
                mWin.showLog((string)mWin.FindResource("there_is_no_task_need_stop"));
                return -1;
            }

            mCts.Cancel();
            mCts = null;
            mGenerateTask = null;

            mWin.showLog((string)mWin.FindResource("stop_task_success"));
            return 0;
        }

        /*
         * 内部逻辑函数
         */
        private void run(CancellationToken ct, string fileStr, string folderStr)
        {
            List<Session> listSessions = loadInputFile(ct, fileStr);
            if (null == listSessions)
            {
                goto safe_exit;
            }

            if (0 > generateCfg(ct, folderStr, listSessions))
            {
                goto safe_exit;
            }

        safe_exit:
            // 执行结束
            mCts = null;
            mGenerateTask = null;
            mWin.Dispatcher.BeginInvoke(new Action(()
                => mWin.notifyFinshed()));
        }

        private List<Session> loadInputFile(CancellationToken ct, string fileStr)
        {
            Workbook excel = new Workbook();
            try
            {
                excel.LoadFromFile(fileStr);
                List<Session> listSessions = loadDatas(ct, excel);
                if (null == listSessions)
                {
                    return null;
                }

                return listSessions;
            }
            catch (System.Exception ex)
            {
                mWin.showLog((string)mWin.FindResource("open_file_err")
                    + ": " + ex.ToString());
                return null;
            }
            finally
            {
                excel.Dispose();
            }
        }

        private List<Session> loadDatas(CancellationToken ct, Workbook excel)
        {
            try
            {
                List<Session> listSessions = new List<Session>();
                foreach (Worksheet sheet in excel.Worksheets)
                {
                    ct.ThrowIfCancellationRequested();

                    if (!mWin.FindResource("sheet_name_datas").Equals(sheet.Name))
                    {
                        continue;
                    }

                    int rowIndex = 0;
                    foreach (CellRange row in sheet.Rows)
                    {
                        ct.ThrowIfCancellationRequested();

                        if (rowIndex++ == 0)
                        {
                            // 默认第一行为标题行
                            continue;
                        }

                        int colIndex = 0;
                        Session session = null;
                        foreach (CellRange col in row.Columns)
                        {
                            ct.ThrowIfCancellationRequested();

                            string display = sheet.Rows[0].Columns[colIndex++].Text;
                            if (string.IsNullOrWhiteSpace(display))
                            {
                                continue;
                            }

                            if (mWin.FindResource("row_name_file").Equals(display))
                            {
                                string file = getSmartFileName(col.Text);
                                if (string.IsNullOrWhiteSpace(file))
                                {
                                    continue;
                                }
                                session = getSessionByFileName(file, listSessions);
                                if (null == session)
                                {
                                    session = new Session();
                                    session.file = file;
                                    listSessions.Add(session);
                                }
                                continue;
                            }

                            if (null == session)
                            {
                                continue;
                            }

                            Key key = new Key();
                            key.display = display;
                            key.name = getMapNameByDisplay(excel, display);
                            key.value = col.Text;
                            session.listKeys.Add(key);
                        }
                    }
                }

                if (0 >= listSessions.Count)
                {
                    return null;
                }
                return listSessions;
            }
            catch (System.Exception ex)
            {
                mWin.showLog((string)mWin.FindResource("read_datas_from_file_failed")
                    + ": " + ex.ToString());
                return null;
            }
        }

        private string getSmartFileName(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
            string extension = System.IO.Path.GetExtension(file);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return fileName + "." + "cfg";
            }

            return fileName + extension;
        }

        private Session getSessionByFileName(String file, List<Session> listSessions)
        {
            foreach (Session session in listSessions)
            {
                if (null != session && file.Equals(session.file))
                {
                    return session;
                }
            }

            return null;
        }

        private string getMapNameByDisplay(Workbook excel, string display)
        {
            try
            {
                foreach (Worksheet sheet in excel.Worksheets)
                {
                    if (!mWin.FindResource("sheet_name_maps").Equals(sheet.Name))
                    {
                        continue;
                    }

                    if (sheet.Columns.Length != 2)
                    {
                        mWin.showLog((string)mWin.FindResource("invalid_maps_sheet"));
                        return null;
                    }
                    CellRange cell = sheet.FindString(display, false, false);
                    if (null == cell)
                    {
                        return null;
                    }

                    return sheet.Rows[cell.Row - 1].Columns[cell.Column].Text;
                }

                return null;
            }
            catch (System.Exception ex)
            {
                mWin.showLog((string)mWin.FindResource("read_maps_from_file_failed")
                    + ": " + ex.ToString());
                return null;
            }
        }

        private int generateCfg(CancellationToken ct, string folderStr, List<Session> listSessions)
        {
            DirectoryInfo folderInfo = new DirectoryInfo(folderStr);
            if (!folderInfo.Exists)
            {
                return -1;
            }

            foreach (Session session in listSessions)
            {
                ct.ThrowIfCancellationRequested();

                if (null == session || string.IsNullOrWhiteSpace(session.file))
                {
                    continue;
                }

                mWin.showLog(mWin.FindResource("generate_file") + ": " + session.file);
                string fileFullName = folderInfo.FullName
                    + System.IO.Path.DirectorySeparatorChar + session.file;
                Encoding encoder = Encoding.UTF8;
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fileFullName, FileMode.OpenOrCreate);
                    fs.Position = fs.Length;
                    foreach (Key key in session.listKeys)
                    {
                        ct.ThrowIfCancellationRequested();

                        string line = "#" + key.display + System.Environment.NewLine;
                        if (string.IsNullOrWhiteSpace(key.name) || "#".Equals(key.value))
                        {
                            line += "#";
                        }
                        line += key.name + " = " + key.value 
                            + System.Environment.NewLine;

                        byte[] bytes = encoder.GetBytes(line);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Flush();
                    }
                }
                catch (System.Exception ex)
                {
                    mWin.showLog(mWin.FindResource("write_file_failed") 
                        + ": " + fileFullName 
                        + "(" + ex + ")");
                    return -1;
                }
                finally
                {
                    if (null != fs)
                    {
                        fs.Flush();
                        fs.Close();
                    }
                }
            }
            return 0;
        }
    }

}
