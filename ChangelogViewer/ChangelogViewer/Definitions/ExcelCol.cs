using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangelogViewer.Definitions
{
    public enum ColId
    {
        NewVersion,
        OldVersion,
        Device,
        Oem,
        Change,
        Svn,
        Git,
        Date,
        Desc,
    }

    public class ExcelCol
    {
        public ColId id;
        public string name;
        public int index;

        public ExcelCol(ColId id, string name)
        {
            this.id = id;
            this.name = name;
            this.index = -1;
        }
    }
}
