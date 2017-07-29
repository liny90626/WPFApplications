using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using PDMTools.datas;
using PDMTools.defined;

namespace PDMTools.controls
{
    class ExcelControl
    {
        private MainWindow mWin;

        public ExcelControl(MainWindow win)
        {
            mWin = win;
        }

        public List<Operate> loadTemplateParams(Operate op)
        {
            if (null == op || Defined.OperateType.LoadTempalteParams != op.type)
            {
                return null;
            }

            return null;
        }
    }
}
