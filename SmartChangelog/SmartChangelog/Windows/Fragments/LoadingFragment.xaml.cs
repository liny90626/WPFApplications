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

namespace SmartChangelog.Windows.Fragments
{
    /// <summary>
    /// LoadingFragment.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingFragment : UserControl
    {
        private MainWindow mWin = null;

        public LoadingFragment(MainWindow win)
        {
            InitializeComponent();

            mWin = win;
        }

        public void ShowProgress(string state)
        {
            this.LoadingState.Content = state;
        }
    }
}
