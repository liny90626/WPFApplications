using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangelogViewer.Windows.Fragments
{
    interface DataInterface
    {
        int SetData(Object data);
        Object GetPrevData();
        Object GetNextData();
    }
}
