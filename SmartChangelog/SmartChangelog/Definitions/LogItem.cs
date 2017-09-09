using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartChangelog.Definitions
{
    public class LogItem
    {
        // 日志类型
        public Constant.LogType type { get; set; }

        // 日志作者
        public string author { get; set; }

        // 日志版本
        public string revision { get; set; }

        // 日志内容
        public string content { get; set; }

        // 日志时间
        public DateTime time { get; set; }
    }
}
