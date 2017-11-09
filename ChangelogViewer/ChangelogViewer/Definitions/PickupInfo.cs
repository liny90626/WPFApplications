using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangelogViewer.Definitions
{
    public class PickupInfo
    {
        public string oemName;
        public string startVersion;
        public string endVersion;

        public List<Changelog> changelogList;
    }
}
