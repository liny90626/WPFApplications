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

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using ChangelogViewer.Windows.Fragments;
using ChangelogViewer.Definitions;

namespace ChangelogViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<UserControl> mListFs = null;
        private int mListIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Prev()
        {
            --mListIndex;
            if (mListIndex < 0)
            {
                // 下标越界, 无法切换
                mListIndex = 0;
                return;
            }

            DataInterface lastI = mListFs[mListIndex + 1] as DataInterface;
            DataInterface curI = mListFs[mListIndex] as DataInterface;
            if (0 != curI.SetData(lastI.GetPrevData())) 
            {
                // 数据不匹配, 无法切换, 再退一个界面
                Prev();
                return;
            }

            this.MainContent.Content = mListFs[mListIndex];
        }

        public void Next(Object data = null)
        {
            ++mListIndex;
            if (mListIndex >= mListFs.Count())
            {
                // 下标越界, 无法切换
                mListIndex = mListFs.Count() - 1;
                return;
            }

            if (null != data)
            {
                DataInterface curI = mListFs[mListIndex] as DataInterface;
                if (0 != curI.SetData(data))
                {
                    // 数据不匹配, 无法切换
                    return;
                }
            }
            else
            {
                DataInterface lastI = mListFs[mListIndex - 1] as DataInterface;
                DataInterface curI = mListFs[mListIndex] as DataInterface;
                if (0 != curI.SetData(lastI.GetNextData()))
                {
                    // 数据不匹配, 无法切换
                    return;
                }
            }

            this.MainContent.Content = mListFs[mListIndex];
        }

        public void SaveAndReload(Changelog changelog)
        {
            mListIndex = 0;
            Next(changelog);
        }

        public void ShowTipMessage(string message)
        {
            this.ShowMessageAsync((string)FindResource("tips"),
                message, MessageDialogStyle.Affirmative,
                new MetroDialogSettings() { AffirmativeButtonText = (string)FindResource("ok") });
        }

        public string ShowSelectFileDialog(string filter)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = filter;
            dialog.RestoreDirectory = true;
            if (System.Windows.Forms.DialogResult.OK != dialog.ShowDialog())
            {
                return null;
            }

            if (string.IsNullOrEmpty(dialog.FileName))
            {
                ShowTipMessage((string)FindResource("selected_file_can_not_be_empty"));
                return null;
            }

            return dialog.FileName;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != mListFs)
            {
                mListFs.Clear();
                mListFs = null;
            }

            mListFs = new List<UserControl>();
            mListIndex = 0;

            mListFs.Add(new ChooseFile(this));
            mListFs.Add(new LoadingFile(this));
            mListFs.Add(new PickupVersion(this));
            mListFs.Add(new LoadingChangelog(this));
            mListFs.Add(new ShowChangelog(this));

            this.MainContent.Content = mListFs[mListIndex];
        }
    }
}
