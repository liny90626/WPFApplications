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
using System.Configuration;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using SmartChangelog.Controls;
using SmartChangelog.Windows.Fragments;
using SmartChangelog.Definitions;

namespace SmartChangelog
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /* Controls */
        private MainControl mMainC = null;

        /* Fragments */
        private IdleFragment mIdleF = null;
        private LoadingFragment mLoadingF = null;
        private QAFragment mQAF = null;
        private ReportFragment mReportF = null;

        /* State Machine */
        private Constant.UiState mUiState = Constant.UiState.Idle;

        public MainWindow()
        {
            InitializeComponent();
            InitControls();
            InitFragments();

            // 版本号, [大版本].[小版本].[与2000/1/1的差值天数].[当天时间刻度]
            this.VersionLabel.Content = (string)FindResource("version") + ": "
                + App.ResourceAssembly.GetName(false).Version;

            ShowState();
        }

        private void InitControls()
        {
            mMainC = new MainControl(this);
        }

        private void InitFragments()
        {
            mIdleF = new IdleFragment(this);
            mLoadingF = new LoadingFragment(this);
            mQAF = new QAFragment(this);
            mReportF = new ReportFragment(this);
        }

        private void ShowState()
        {
            switch (mUiState)
            {
                case Constant.UiState.Report:
                    {
                        // Fragments
                        mIdleF.DisableAll();
                        mQAF.DisableAll();
                        mReportF.EnableAll();
                        this.MainContent.Content = mReportF;

                        this.NextBtn.IsEnabled = false;
                        this.NextBtn.Content = (string)FindResource("next");

                        this.PrevBtn.IsEnabled = false;
                        this.PrevBtn.Content = (string)FindResource("prev");

                        this.ResetBtn.IsEnabled = true;
                    }
                    break;

                case Constant.UiState.QuestionAndAnswer:
                    {
                        // Fragments
                        mIdleF.DisableAll();
                        mQAF.EnableAll();
                        mReportF.DisableAll();
                        this.MainContent.Content = mQAF;

                        // Window-Self
                        if (mQAF.IsHead())
                        {
                            this.NextBtn.IsEnabled = true;
                            this.NextBtn.Content = (string)FindResource("next");

                            this.PrevBtn.IsEnabled = false;
                            this.PrevBtn.Content = (string)FindResource("prev");
                        }
                        else if (mQAF.IsTail())
                        {
                            this.NextBtn.IsEnabled = true;
                            this.NextBtn.Content = (string)FindResource("finish");

                            this.PrevBtn.IsEnabled = true;
                            this.PrevBtn.Content = (string)FindResource("prev");
                        }
                        else
                        {
                            this.NextBtn.IsEnabled = true;
                            this.NextBtn.Content = (string)FindResource("next");

                            this.PrevBtn.IsEnabled = true;
                            this.PrevBtn.Content = (string)FindResource("prev");
                        }

                        this.ResetBtn.IsEnabled = true;
                    }
                    break;

                case Constant.UiState.Reporting:
                case Constant.UiState.Loading:
                    {
                        // Fragments
                        mIdleF.DisableAll();
                        mQAF.DisableAll();
                        mReportF.DisableAll();
                        this.MainContent.Content = mLoadingF;

                        // Window-Self
                        this.NextBtn.IsEnabled = false;
                        this.NextBtn.Content = (string)FindResource("next");

                        this.PrevBtn.IsEnabled = false;
                        this.PrevBtn.Content = (string)FindResource("prev");

                        this.ResetBtn.IsEnabled = true;
                    }
                    break;

                case Constant.UiState.IdleReady: 
                    {
                        // Fragments
                        mIdleF.EnableAll();
                        mQAF.DisableAll();
                        mReportF.DisableAll();
                        this.MainContent.Content = mIdleF;

                        // Window-Self
                        this.NextBtn.IsEnabled = true;
                        this.NextBtn.Content = (string)FindResource("start");

                        this.PrevBtn.IsEnabled = false;
                        this.PrevBtn.Content = (string)FindResource("prev");

                        this.ResetBtn.IsEnabled = false;
                    }
                    break;

                case Constant.UiState.Idle:
                default: 
                    {
                        // Fragments
                        mIdleF.EnableAll();
                        mQAF.DisableAll();
                        mReportF.DisableAll();
                        this.MainContent.Content = mIdleF;

                        // Window-Self
                        this.NextBtn.IsEnabled = false;
                        this.NextBtn.Content = (string)FindResource("start");

                        this.PrevBtn.IsEnabled = false;
                        this.PrevBtn.Content = (string)FindResource("prev");

                        this.ResetBtn.IsEnabled = false;
                    }
                    break;
            }
        }

        private void ReportLoadingProgress(string state)
        {
            switch (mUiState)
            {
                case Constant.UiState.Loading:
                    mLoadingF.ShowProgress(state);
                    break;

                default:
                    // 非法的状态
                    return;
            }
        }

        private void NotifyLoadingFinished(bool success, string err, Changelog svnChangelog, Changelog gitChangelog)
        {
            switch (mUiState)
            {
                case Constant.UiState.Loading:
                    {
                        if (success)
                        {
                            if ((null != svnChangelog && svnChangelog.unkownList.Count > 0)
                                || (null != gitChangelog && gitChangelog.unkownList.Count > 0))
                            {
                                // 存在未知项, 进入QA环节
                                mUiState = Constant.UiState.QuestionAndAnswer;
                                mQAF.SetData(svnChangelog, gitChangelog);
                            }
                            else
                            {
                                mUiState = Constant.UiState.Reporting;
                                mMainC.ReportDataAsync(svnChangelog, gitChangelog);
                            }
                        }
                        else
                        {
                            ShowErrorMessage(err);
                            mUiState = Constant.UiState.IdleReady;
                        }
                    }
                    break;

                default:
                    // 非法的状态
                    return;
            }

            ShowState();
        }

        private void NotifyReportFinished(bool success, string err, 
            Changelog allChangelog, Changelog svnChangelog, Changelog gitChangelog)
        {
            switch (mUiState)
            {
                case Constant.UiState.Reporting:
                    {
                        if (success)
                        {
                            mUiState = Constant.UiState.Report;
                            mReportF.SetData(allChangelog, svnChangelog, gitChangelog);
                        }
                        else
                        {
                            ShowErrorMessage(err);
                            mUiState = Constant.UiState.IdleReady;
                        }
                    }
                    break;

                default:
                    // 非法的状态
                    return;
            }

            ShowState();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Basic Settings
            this.ConfigSvnServer.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnServerAddr];
            this.ConfigGitServer.Text = ConfigurationManager.AppSettings[Constant.Cfg.GitServerAddr];

            // Regx Svn Settings
            this.ConfigSvnRegxEventId.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxEventId];
            this.ConfigSvnRegxContent.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxContent];

            this.ConfigSvnRegxAdd.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxAdd];
            this.ConfigSvnRegxBack.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxBack];
            this.ConfigSvnRegxOptimize.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxOptimize];
            this.ConfigSvnRegxFix.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxFix];
            this.ConfigSvnRegxOem.Text = ConfigurationManager.AppSettings[Constant.Cfg.SvnRegxOem];
        }

        private void ConfigSvnRegxEventId_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnRegxEventId].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigSvnRegxContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnRegxContent].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigSvnServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnServerAddr].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigGitServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.GitServerAddr].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigSvnRegxAdd_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnRegxAdd].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigSvnRegxBack_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnRegxBack].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigSvnRegxOptimize_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnRegxOptimize].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigSvnRegxFix_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnRegxFix].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void ConfigSvnRegxOem_TextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[Constant.Cfg.SvnRegxOem].Value = ((TextBox)sender).Text;
            config.Save();
            ConfigurationManager.RefreshSection(Constant.Cfg.Name);
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (mUiState)
            {
                case Constant.UiState.QuestionAndAnswer:
                    {
                        if (mQAF.IsTail())
                        {
                            if (true == ShowConfimrMessage(
                                (string)FindResource("will_enter_reporting"),
                                (string)FindResource("are_you_sure_to_finish_this_job")))
                            {
                                mUiState = Constant.UiState.Reporting;

                                Changelog svnChangelog = null;
                                Changelog gitChangelog = null;
                                mQAF.GetData(out svnChangelog, out gitChangelog);
                                mMainC.ReportDataAsync(svnChangelog, gitChangelog);
                            }
                        }
                        else
                        {
                            mQAF.ShowNextData();
                        }
                    }
                    break;

                case Constant.UiState.IdleReady:
                    {
                        mUiState = Constant.UiState.Loading;
                        Dictionary<Constant.DictName, string> dict = new Dictionary<Constant.DictName, string>();
                        dict.Add(Constant.DictName.SvnBranchName, mIdleF.SvnBranchName.Text);
                        dict.Add(Constant.DictName.SvnLastVersion, mIdleF.SvnLastVersion.Text);
                        dict.Add(Constant.DictName.SvnCurrentVersion, mIdleF.SvnCurrentVersion.Text);
                        dict.Add(Constant.DictName.GitRepositoryName, mIdleF.GitRepositoryName.Text);
                        dict.Add(Constant.DictName.GitBranchName, mIdleF.GitBranchName.Text);
                        dict.Add(Constant.DictName.GitLastVersion, mIdleF.GitLastVersion.Text);
                        dict.Add(Constant.DictName.GitCurrentVersion, mIdleF.GitCurrentVersion.Text);
                        mMainC.LoadDataAsync(dict);
                    }
                    break;

                default:
                    {
                        ShowErrorMessage(FindResource("state_machine_error")
                            + "(" + mUiState + ")");
                    }
                    return;
            }

            ShowState();
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (mUiState)
            {
                case Constant.UiState.Report:
                    { 
                    }
                    break;

                case Constant.UiState.QuestionAndAnswer:
                    {
                        mQAF.ShowPrevData();
                    }
                    break;

                default:
                    {
                        ShowErrorMessage(FindResource("state_machine_error")
                            + "(" + mUiState + ")");
                    }
                    return;
            }

            ShowState();
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (mUiState)
            {
                case Constant.UiState.Report:
                case Constant.UiState.QuestionAndAnswer:
                case Constant.UiState.Loading:
                    {
                        // 停止任务
                        mMainC.StopLoadTask();
                        mMainC.StopReportTask();

                        mUiState = Constant.UiState.IdleReady;
                    }
                    break;

                default:
                    {
                        ShowErrorMessage(FindResource("state_machine_error")
                            + "(" + mUiState + ")");
                    }
                    return;
            }

            ShowState();
        }

        /************************************************************************/
        /* 对外公开APIs                                                         */
        /************************************************************************/
        public void ShowTipMessage(string message)
        {
            this.ShowMessageAsync((string)FindResource("tips"),
                message, MessageDialogStyle.Affirmative,
                new MetroDialogSettings() { AffirmativeButtonText = (string)FindResource("ok") });
        }

        public void ShowErrorMessage(string message)
        {
            this.ShowMessageAsync((string)FindResource("error"),
                message, MessageDialogStyle.Affirmative,
                new MetroDialogSettings() { AffirmativeButtonText = (string)FindResource("ok") });
        }

        public bool ShowConfimrMessage(string title, string message)
        {
            MessageDialogResult result = this.ShowModalMessageExternal(
                title, message, 
                MessageDialogStyle.AffirmativeAndNegative);
            return (result == MessageDialogResult.Affirmative);
        }

        public void NotifyIdleFragmentReady(bool ready) 
        {
            switch (mUiState) 
            {
                case Constant.UiState.IdleReady:
                    {
                        if (!ready)
                        {
                            mUiState = Constant.UiState.Idle;
                        }
                    }
                    break;

                case Constant.UiState.Idle:
                    {
                        if (ready)
                        {
                            mUiState = Constant.UiState.IdleReady;
                        }
                    }
                    break;

                default:
                    // 非法的状态
                    return;
            }

            ShowState();
        }

        public void ReportLoadingProgressAsync(string state)
        {
            this.Dispatcher.BeginInvoke(new Action(()
                => ReportLoadingProgress(state)));
        }

        public void NotifyLoadingFinishedAsync(bool success, string err, Changelog svnChangelog, Changelog gitChangelog)
        {
            this.Dispatcher.BeginInvoke(new Action(()
                => NotifyLoadingFinished(success, err, svnChangelog, gitChangelog)));
        }

        public void NotifyReportFinishedAsync(bool success, string err,
            Changelog allChangelog, Changelog svnChangelog, Changelog gitChangelog)
        {
            this.Dispatcher.BeginInvoke(new Action(()
                => NotifyReportFinished(success, err, allChangelog, svnChangelog, gitChangelog)));
        }
    }
}
