using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CfgConverter.defineds
{
    class Session
    {
        public string file { set; get; }

        public List<Key> listKeys = new List<Key>();
    }
}
