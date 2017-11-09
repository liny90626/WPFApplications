using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

using Spire.Xls;

using ChangelogViewer.Definitions;

namespace ChangelogViewer.Windows.Fragments
{
    /// <summary>
    /// LoadingFile.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingFile : UserControl, DataInterface
    {
        private MainWindow mWin = null;

        private string mFilePath = null;
        private List<Changelog> mChangelogList = null;

        private Changelog mSaveChangelog = null;

        private Task mLoadingTask = null;
        private CancellationTokenSource mCts = null;

        public LoadingFile(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public int SetData(Object data)
        {
            if (data is Changelog)
            {
                mSaveChangelog = data as Changelog;
                return 0;
            }
            mSaveChangelog = null;

            if (!(data is string))
            {
                return -1;
            }

            mFilePath = data as string;
            return 0;
        }

        public Object GetPrevData()
        {
            return mFilePath;
        }

        public Object GetNextData()
        {
            return mChangelogList;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != mSaveChangelog)
            {
                StartSaving();
            }
            else
            {
                StartLoading();
            }
        }

        private void StartLoading()
        {
            StopLoading();

            mCts = new CancellationTokenSource();
            mLoadingTask = new Task(() => Loading(mCts.Token, mFilePath), mCts.Token);
            mLoadingTask.Start();
        }

        private void StartSaving()
        {
            StopLoading();

            mCts = new CancellationTokenSource();
            mLoadingTask = new Task(() => Saving(mCts.Token, mFilePath, mSaveChangelog), mCts.Token);
            mLoadingTask.Start();
        }

        public void StopLoading()
        {
            if (null == mCts)
            {
                return;
            }

            mCts.Cancel();
            mCts = null;
            mLoadingTask = null;
        }

        private void LoadingCompleted(bool success, string failedReason = null)
        {
            if (success)
            {
                mWin.Dispatcher.BeginInvoke(new Action(()
                    => mWin.Next()));
            }
            else
            {
                mWin.Dispatcher.BeginInvoke(new Action(()
                    => 
                    {
                        mWin.ShowTipMessage(failedReason);
                        mWin.Prev();
                    }));
            }
        }

        private void ShowLoadingState(string state)
        {
            mWin.Dispatcher.BeginInvoke(new Action(()
                => {
                this.LoadingState.Content = state;
            }));
        }

        private void Loading(CancellationToken ct, string filePath)
        {
            // 开始文件加载
            ShowLoadingState((string)mWin.FindResource("loading_file"));
            Workbook excel = new Workbook();
            try
            {
                excel.LoadFromFile(filePath, true);
            }
            catch (Exception)
            {
                LoadingCompleted(false, (string)mWin.FindResource("load_file_failed"));
                return;
            }

            // 开始数据解析
            ShowLoadingState((string)mWin.FindResource("parsing_file"));
            if (0 != ParseExcel(excel))
            {
                // 错误提示已经在ParseExcel中处理了
                return;
            }

            LoadingCompleted(true);
        }

        private int ParseExcel(Workbook excel)
        {
            try
            {
                mChangelogList = new List<Changelog>();
                foreach (Worksheet sheet in excel.Worksheets)
                {
                    // 假设excel中sheet1为实际数据, sheet2为标记数据
                    if (sheet.Name.Equals("Sheet1", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowLoadingState((string)mWin.FindResource("parsing_file_data"));
                        if (0 != ParseDataSheet(sheet, mChangelogList))
                        {
                            LoadingCompleted(false, (string)mWin.FindResource("parse_file_data_failed"));
                            return -1;
                        }
                    }
                    else if (sheet.Name.Equals("Sheet2", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowLoadingState((string)mWin.FindResource("parsing_file_record"));
                        if (0 != ParseRecordSheet(sheet, mChangelogList))
                        {
                            LoadingCompleted(false, (string)mWin.FindResource("parse_file_record_failed"));
                            return -1;
                        }
                    }
                }
            }
            catch (Exception)
            {
                LoadingCompleted(false, (string)mWin.FindResource("parse_file_failed"));
                return -1;
            }

            return 0;
        }

        private int ParseDataSheet(Worksheet sheet, List<Changelog> changelogList)
        {
            List<ExcelCol> excelColList = BuildExcelDataColList();

            int index = 0;
            foreach (CellRange column in sheet.Rows[0])
            {
                // 遍历第一行的每一列
                foreach (ExcelCol excelCol in excelColList)
                {
                    if (excelCol.name.Equals(column.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        excelCol.index = index;
                        break;
                    }
                }
                ++index;
            }

            bool skipFirst = false;
            foreach (CellRange row in sheet.Rows)
            {
                if (!skipFirst)
                {
                    // 跳过第一行
                    skipFirst = true;
                    continue;
                }

                Changelog changelog = new Changelog();
                foreach (ExcelCol excelCol in excelColList)
                {
                    if (excelCol.index < 0)
                    {
                        continue;
                    }

                    string text = row.Columns[excelCol.index].RichText.Text.Trim();
                    if (ColId.NewVersion == excelCol.id && string.IsNullOrWhiteSpace(text))
                    {
                        // 版本不可为空, 否则视为空行
                        break;
                    }

                    switch (excelCol.id)
                    {
                        case ColId.NewVersion:
                            changelog.newVersionStr = text;
                            changelog.newVersionInt = ParseVersionInt(changelog.newVersionStr);
                            break;

                        case ColId.OldVersion:
                            changelog.oldVersionStr = text;
                            changelog.oldVersionInt = ParseVersionInt(changelog.oldVersionStr);
                            break;

                        case ColId.Device:
                            changelog.deviceList = ParseDeviceList(text);
                            break;

                        case ColId.Oem:
                            changelog.oemList = ParseOemList(text);
                            break;

                        case ColId.Change:
                            changelog.changeAddList = ParseChangeList(text, (string)mWin.FindResource("add"));
                            changelog.changeFixList = ParseChangeList(text, (string)mWin.FindResource("fix"));
                            changelog.changeOptList = ParseChangeList(text, (string)mWin.FindResource("optimize"));
                            changelog.changeOemList = ParseChangeList(text, (string)mWin.FindResource("oem"));
                            changelog.changeOthList = ParseChangeList(text, null, true);
                            break;

                        case ColId.Svn:
                            changelog.svnAddr = ParseSvnAddr(text);
                            changelog.svnBranch = ParseSvnBranch(text);
                            changelog.svnRevision = ParseSvnRevision(text);
                            break;

                        case ColId.Git:
                            changelog.gitAddr = ParseGitAddr(text);
                            changelog.gitBranch = ParseGitBranch(text);
                            changelog.gitRevision = ParseGitRevision(text);
                            break;

                        case ColId.Date:
                            changelog.date = text;
                            break;

                        case ColId.Desc:
                            changelog.desc = text;
                            break;

                        default:
                            // 未识别字段!
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(changelog.newVersionStr))
                {
                    changelogList.Add(changelog);
                }
            }

            return 0;
        }

        private int ParseRecordSheet(Worksheet sheet, List<Changelog> changelogList)
        {
            // 这里假设sheet2格式固定, 无标题行, 仅一列, 用于记录已测试的修改记录
            string change = null;
            foreach (CellRange row in sheet.Rows)
            {
                try
                {
                    change = row.Columns[0].RichText.Text.Trim();
                    if (string.IsNullOrWhiteSpace(change))
                    {
                        continue;
                    }
                }
                catch (System.Exception)
                {
                    continue;
                }

                foreach (Changelog changelog in changelogList)
                {
                    foreach (ChangeRecord changeRecord in changelog.changeAddList)
                    {
                        if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                        {
                            changeRecord.tested = true;
                        }
                    }

                    foreach (ChangeRecord changeRecord in changelog.changeFixList)
                    {
                        if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                        {
                            changeRecord.tested = true;
                        }
                    }

                    foreach (ChangeRecord changeRecord in changelog.changeOptList)
                    {
                        if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                        {
                            changeRecord.tested = true;
                        }
                    }

                    foreach (ChangeRecord changeRecord in changelog.changeOemList)
                    {
                        if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                        {
                            changeRecord.tested = true;
                        }
                    }

                    foreach (ChangeRecord changeRecord in changelog.changeOthList)
                    {
                        if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                        {
                            changeRecord.tested = true;
                        }
                    }
                }
            }
            return 0;
        }

        private List<ExcelCol> BuildExcelDataColList()
        {
            List<ExcelCol> excelColList = new List<ExcelCol>();
            excelColList.Add(new ExcelCol(ColId.NewVersion, 
                (string)mWin.FindResource("excel_col_new_version")));
            excelColList.Add(new ExcelCol(ColId.OldVersion, 
                (string)mWin.FindResource("excel_col_old_version")));
            excelColList.Add(new ExcelCol(ColId.Device, 
                (string)mWin.FindResource("excel_col_device")));
            excelColList.Add(new ExcelCol(ColId.Oem, 
                (string)mWin.FindResource("excel_col_oem")));
            excelColList.Add(new ExcelCol(ColId.Change, 
                (string)mWin.FindResource("excel_col_change")));
            excelColList.Add(new ExcelCol(ColId.Svn, 
                (string)mWin.FindResource("excel_col_svn")));
            excelColList.Add(new ExcelCol(ColId.Git, 
                (string)mWin.FindResource("excel_col_git")));
            excelColList.Add(new ExcelCol(ColId.Date, 
                (string)mWin.FindResource("excel_col_date")));
            excelColList.Add(new ExcelCol(ColId.Desc, 
                (string)mWin.FindResource("excel_col_desc")));
            return excelColList;
        }

        private int[] ParseVersionInt(string versionStr)
        {
            if (string.IsNullOrWhiteSpace(versionStr))
            {
                return null;
            }

            string[] versionTmp = versionStr.Split('.');
            if (versionTmp.Length != 4)
            {
                return null;
            }

            int[] versionInt = new int[versionTmp.Length];
            for (int i = 0; i < versionInt.Length; ++i)
            {
                try
                {
                    versionInt[i] = int.Parse(versionTmp[i]);
                }
                catch (System.Exception)
                {
                    // 解析失败, 可能含有x信息
                    versionInt[i] = -1;
                }
            }

            return versionInt;
        }

        private List<string> ParseDeviceList(string device)
        {
            // 这里假设device列表与oem列表格式一致
            return ParseOemList(device);
        }

        private List<string> ParseOemList(string oem)
        {
            if (string.IsNullOrWhiteSpace(oem))
            {
                return null;
            }

            List<string> oemList = new List<string>();
            string[] oems = oem.Split('/');
            for (int i = 0; i < oems.Length; ++i)
            {
                // 统一大小写
                oemList.Add(oems[i].Trim().ToLower());
            }

            return oemList;
        }

        private List<ChangeRecord> ParseChangeList(string change, string startWords, bool other = false)
        {
            string[] changes = change.Split('\n');
            if (changes.Length < 1)
            {
                return null;
            }

            List<ChangeRecord> changeList = new List<ChangeRecord>();
            bool record = other;
            for (int i = 0; i < changes.Length; ++i)
            {
                if (string.IsNullOrWhiteSpace(changes[i]))
                {
                    continue;
                }

                if (changes[i].Length <= 4)
                {
                    if (other)
                    {
                        // Other记录下出现了关键行, 这不符合预期, 直接退出
                        return changeList;
                    }

                    // 视为关键词行
                    if (record || changes[i].StartsWith(startWords))
                    {
                        record = !record;
                    }
                    continue;
                }

                if (record)
                {
                    changeList.Add(new ChangeRecord(changes[i].Trim()));
                }
            }

            return changeList;
        }

        private string ParseSvnAddr(string svn)
        {
            Regex rgx = new Regex(@"addr:(.*)", RegexOptions.IgnoreCase);
            string addr = rgx.Match(svn).ToString().Trim();
            if (string.IsNullOrWhiteSpace(addr))
            {
                return null;
            }

            return addr.Substring(addr.IndexOf(':')+1).Trim();
        }

        private string ParseSvnBranch(string svn)
        {
            Regex rgx = new Regex(@"branch:(.*)", RegexOptions.IgnoreCase);
            string branch = rgx.Match(svn).ToString().Trim();
            if (string.IsNullOrWhiteSpace(branch))
            {
                return null;
            }

            return branch.Substring(branch.IndexOf(':') + 1).Trim();
        }

        private string ParseSvnRevision(string svn)
        {
            Regex rgx = new Regex(@"revision:(.*)", RegexOptions.IgnoreCase);
            string revision = rgx.Match(svn).ToString().Trim();
            if (string.IsNullOrWhiteSpace(revision))
            {
                return null;
            }

            return revision.Substring(revision.IndexOf(':') + 1).Trim();
        }

        private string ParseGitAddr(string git)
        {
            Regex rgx = new Regex(@"addr:(.*)", RegexOptions.IgnoreCase);
            string addr = rgx.Match(git).ToString().Trim();
            if (string.IsNullOrWhiteSpace(addr))
            {
                return null;
            }

            return addr.Substring(addr.IndexOf(':') + 1).Trim();
        }

        private string ParseGitBranch(string git)
        {
            Regex rgx = new Regex(@"branch:(.*)", RegexOptions.IgnoreCase);
            string branch = rgx.Match(git).ToString().Trim();
            if (string.IsNullOrWhiteSpace(branch))
            {
                return null;
            }

            return branch.Substring(branch.IndexOf(':') + 1).Trim();
        }

        private string ParseGitRevision(string git)
        {
            Regex rgx = new Regex(@"commit:(.*)", RegexOptions.IgnoreCase);
            string commit = rgx.Match(git).ToString().Trim();
            if (string.IsNullOrWhiteSpace(commit))
            {
                return null;
            }

            return commit.Substring(commit.IndexOf(':') + 1).Trim();
        }

        private void Saving(CancellationToken ct, string filePath, Changelog saveChangelog)
        {
            // 开始文件加载
            ShowLoadingState((string)mWin.FindResource("saving_file"));
            Workbook excel = new Workbook();
            try
            {
                excel.LoadFromFile(filePath, true);
            }
            catch (Exception)
            {
                LoadingCompleted(false, (string)mWin.FindResource("save_file_failed"));
                return;
            }

            foreach (Worksheet sheet in excel.Worksheets)
            {
                // 假设excel中sheet1为实际数据, sheet2为标记数据
                if (sheet.Name.Equals("Sheet2", StringComparison.OrdinalIgnoreCase))
                {
                    ShowLoadingState((string)mWin.FindResource("saving_file_record"));
                    if (0 != SaveRecordSheet(sheet, saveChangelog))
                    {
                        LoadingCompleted(false, (string)mWin.FindResource("save_file_record"));
                        return;
                    }
                }
            }

            // 统一保存
            excel.Save();

            // 重新加载进内存
            Loading(ct, filePath);
        }

        private int SaveRecordSheet(Worksheet sheet, Changelog saveChangelog)
        {
            // 清空已测->未测的行
            ClearRow(sheet, saveChangelog);

            // 添加未测->已测的行
            InstertRow(sheet, saveChangelog);
            return 0;
        }

        private int ClearRow(Worksheet sheet, Changelog saveChangelog)
        {
            string change = null;
            foreach (CellRange row in sheet.Rows)
            {
                try
                {
                    change = row.Columns[0].RichText.Text.Trim();
                    if (string.IsNullOrWhiteSpace(change))
                    {
                        continue;
                    }
                }
                catch (System.Exception)
                {
                    continue;
                }

                foreach (ChangeRecord changeRecord in saveChangelog.changeAddList)
                {
                    if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                    {
                        // 此时, 若tested为false, 需要删除该行
                        if (!changeRecord.tested)
                        {
                            row.ClearContents();
                        }

                        // 标记该项为false, 后续添加时将不会去操作
                        changeRecord.tested = false;
                    }
                }

                foreach (ChangeRecord changeRecord in saveChangelog.changeFixList)
                {
                    if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                    {
                        // 此时, 若tested为false, 需要删除该行
                        if (!changeRecord.tested)
                        {
                            row.ClearContents();
                        }

                        // 标记该项为false, 后续添加时将不会去操作
                        changeRecord.tested = false;
                    }
                }

                foreach (ChangeRecord changeRecord in saveChangelog.changeOptList)
                {
                    if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                    {
                        // 此时, 若tested为false, 需要删除该行
                        if (!changeRecord.tested)
                        {
                            row.ClearContents();
                        }

                        // 标记该项为false, 后续添加时将不会去操作
                        changeRecord.tested = false;
                    }
                }

                foreach (ChangeRecord changeRecord in saveChangelog.changeOemList)
                {
                    if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                    {
                        // 此时, 若tested为false, 需要删除该行
                        if (!changeRecord.tested)
                        {
                            row.ClearContents();
                        }

                        // 标记该项为false, 后续添加时将不会去操作
                        changeRecord.tested = false;
                    }
                }

                foreach (ChangeRecord changeRecord in saveChangelog.changeOthList)
                {
                    if (change.Equals(changeRecord.change, StringComparison.OrdinalIgnoreCase))
                    {
                        // 此时, 若tested为false, 需要删除该行
                        if (!changeRecord.tested)
                        {
                            row.ClearContents();
                        }

                        // 标记该项为false, 后续添加时将不会去操作
                        changeRecord.tested = false;
                    }
                }
            }

            return 0;
        }

        private int InstertRow(Worksheet sheet, Changelog saveChangelog)
        {
            foreach (ChangeRecord changeRecord in saveChangelog.changeAddList)
            {
                if (changeRecord.tested)
                {
                    InstertRow(sheet, changeRecord.change);
                    changeRecord.tested = false;
                }
            }

            foreach (ChangeRecord changeRecord in saveChangelog.changeFixList)
            {
                if (changeRecord.tested)
                {
                    InstertRow(sheet, changeRecord.change);
                    changeRecord.tested = false;
                }
            }

            foreach (ChangeRecord changeRecord in saveChangelog.changeOptList)
            {
                if (changeRecord.tested)
                {
                    InstertRow(sheet, changeRecord.change);
                    changeRecord.tested = false;
                }
            }

            foreach (ChangeRecord changeRecord in saveChangelog.changeOemList)
            {
                if (changeRecord.tested)
                {
                    InstertRow(sheet, changeRecord.change);
                    changeRecord.tested = false;
                }
            }

            foreach (ChangeRecord changeRecord in saveChangelog.changeOthList)
            {
                if (changeRecord.tested)
                {
                    InstertRow(sheet, changeRecord.change);
                    changeRecord.tested = false;
                }
            }
            return 0;
        }

        private int InstertRow(Worksheet sheet, string change)
        {
            string text = null;
            foreach (CellRange row in sheet.Rows)
            {
                try
                {
                    text = row.Columns[0].RichText.Text.Trim();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        row.Text = change;
                        return 0;
                    }
                }
                catch (System.Exception)
                {
                    row.Text = change;
                    return 0;
                }
            }

            // 增加到最后一行
            sheet.InsertRow(sheet.Rows.Count() + 1);
            sheet.Rows.Last().Text = change;
            return 0;
        }

    }
}
