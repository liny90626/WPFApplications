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

namespace ChangelogViewer.Windows.Fragments
{
    /// <summary>
    /// ChooseFile.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseFile : UserControl, DataInterface
    {
        private MainWindow mWin = null;

        private string mFilePath = null;

        public ChooseFile(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public int SetData(Object data)
        {
            return 0;
        }

        public Object GetPrevData()
        {
            return null;
        }

        public Object GetNextData()
        {
            return mFilePath;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 版本号, [大版本].[小版本].[与2000/1/1的差值天数].[当天时间刻度]
            this.VersionLabel.Content = (string)mWin.FindResource("version") + ": "
                + App.ResourceAssembly.GetName(false).Version;
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            mFilePath = mWin.ShowSelectFileDialog((string)FindResource("filter_changelog_file"));
            if (string.IsNullOrWhiteSpace(mFilePath))
            {
                return;
            }

            mWin.Next();
        }
 
    }
}
