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

using AForge.Neuro;
using AForge.Neuro.Learning;

namespace SmartChangelog.Windows.Fragments
{
    /// <summary>
    /// IdleFragment.xaml 的交互逻辑
    /// </summary>
    public partial class IdleFragment : UserControl
    {
        private MainWindow mWin = null;

        private bool mIsSvnBranchNameReady = false;
        private bool mIsSvnLastVersionReady = false;
        private bool mIsSvnCurrentVersionReady = false;

        private bool mIsGitRepositoryNameReady = false;
        private bool mIsGitBranchNameReady = false;
        private bool mIsGitLastVersionReady = false;
        private bool mIsGitCurrentVersionReady = false;

        public IdleFragment(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public void EnableAll()
        {
            this.SvnBranchName.IsEnabled = true;
            this.SvnLastVersion.IsEnabled = true;
            this.SvnCurrentVersion.IsEnabled = true;

            this.GitRepositoryName.IsEnabled = true;
            this.GitBranchName.IsEnabled = true;
            this.GitLastVersion.IsEnabled = true;
            this.GitCurrentVersion.IsEnabled = true;
        }

        public void DisableAll()
        {
            this.SvnBranchName.IsEnabled = false;
            this.SvnLastVersion.IsEnabled = false;
            this.SvnCurrentVersion.IsEnabled = false;

            this.GitRepositoryName.IsEnabled = false;
            this.GitBranchName.IsEnabled = false;
            this.GitLastVersion.IsEnabled = false;
            this.GitCurrentVersion.IsEnabled = false;
        }

        private void SvnBranchName_TextChanged(object sender, TextChangedEventArgs e)
        {
            mIsSvnBranchNameReady = !string.IsNullOrWhiteSpace((sender as TextBox).Text);
            NotifyReady();
        }

        private void SvnLastVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            mIsSvnLastVersionReady = LimitNumber(sender as TextBox, e);
            NotifyReady();
        }

        private void SvnCurrentVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            mIsSvnCurrentVersionReady = LimitNumber(sender as TextBox, e);
            NotifyReady();
        }

        private void GitRepositoryName_TextChanged(object sender, TextChangedEventArgs e)
        {
            mIsGitRepositoryNameReady = !string.IsNullOrWhiteSpace((sender as TextBox).Text);
            NotifyReady();
        }

        private void GitBranchName_TextChanged(object sender, TextChangedEventArgs e)
        {
            mIsGitBranchNameReady = !string.IsNullOrWhiteSpace((sender as TextBox).Text);
            NotifyReady();
        }

        private void GitLastVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            string content = (sender as TextBox).Text.Trim();
            if (!string.IsNullOrWhiteSpace(content)
                && (40 == content.Length || 7 == content.Length))
            {
                mIsGitLastVersionReady = true;
            }
            else
            {
                mIsGitLastVersionReady = false;
            }
            NotifyReady();
        }

        private void GitCurrentVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            string content = (sender as TextBox).Text.Trim();
            if (!string.IsNullOrWhiteSpace(content)
                && (40 == content.Length || 7 == content.Length))
            {
                mIsGitCurrentVersionReady = true;
            }
            else
            {
                mIsGitCurrentVersionReady = false;
            }
            NotifyReady();
        }

        private bool LimitNumber(TextBox textBox, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox.Text))
            {
                e.Handled = true;
                return false;
            }

            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                long tmpOut = 0;
                if (!long.TryParse(textBox.Text, out tmpOut))
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                    return false;
                }
            }
            return true;
        }

        private void NotifyReady()
        {
            mWin.NotifyIdleFragmentReady(
                (mIsSvnBranchNameReady && mIsSvnLastVersionReady && mIsSvnCurrentVersionReady)
                || (mIsGitRepositoryNameReady && mIsGitBranchNameReady
                        && mIsGitLastVersionReady && mIsGitCurrentVersionReady));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
#if false
            // 测试代码
            this.SvnBranchName.Text = "r52-v6.0";
            this.SvnLastVersion.Text = "36420";
            this.SvnCurrentVersion.Text = "36421";
            mWin.NotifyIdleFragmentReady(true);
#endif
        }
    }
}
