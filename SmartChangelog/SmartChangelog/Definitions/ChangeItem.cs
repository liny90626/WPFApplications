using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartChangelog.Definitions
{
    public class ChangeItem
    {
        // 改变类型
        public Constant.ChangeType type { get; set; }

        // 提交者
        public string author { get; set; }

        // 提交版本
        public string revision { get; set; }

        // 事件Id
        public string eventId { get; set; }

        // 内容
        public string content { get; set; }

        // 对应的log
        public LogItem log { get; set; }
    }
}
