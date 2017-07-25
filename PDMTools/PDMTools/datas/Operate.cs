using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDMTools.defined;

namespace PDMTools.datas
{
	public class Operate
	{
        public Defined.OperateType type { set; get; }

        public string key { set; get; }
        public string value { set; get; }
	}
}
