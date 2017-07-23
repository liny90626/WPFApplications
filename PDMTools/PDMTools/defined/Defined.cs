using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDMTools.defined
{
    public class Defined
    {
        public enum UiState 
        {
            Idle = 0,
            SelectedTemplate,
            SelectedFirmware,
            SelectedTool,
            SelectedFirmwareAndTool,
            Doing,
        }
    }
}
