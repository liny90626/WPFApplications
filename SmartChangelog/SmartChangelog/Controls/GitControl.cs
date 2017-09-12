using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SmartChangelog.Definitions;

namespace SmartChangelog.Controls
{
    class GitControl
    {
        private MainWindow mWin = null;

        public GitControl(MainWindow win)
        {
            mWin = win;
        }

        public void LearnData(Changelog changelog, out LearnStatistics statistics)
        {
            statistics = new LearnStatistics();

            statistics.StatisticsChangeTypeCnt = 0;
            statistics.StatisticsChangeTypeAccuracy = 0.0;
        }
    }
}
