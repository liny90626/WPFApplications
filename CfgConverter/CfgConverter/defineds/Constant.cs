using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CfgConverter.defineds
{
    class Constant
    {
        public enum UiState
        {
            Idle = 0,
            InputFileReady,
            OutputFolderReady,
            AllReady,
            Working,
        }
    }
}
