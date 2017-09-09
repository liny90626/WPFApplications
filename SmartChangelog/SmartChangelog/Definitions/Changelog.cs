using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartChangelog.Definitions
{
    public class Changelog
    {
        // 未知
        public List<ChangeItem> unkownList { get; set; }

        // 新增
        public List<ChangeItem> addList { get; set; }

        // 回退
        public List<ChangeItem> backList { get; set; }

        // 优化
        public List<ChangeItem> optimizeList { get; set; }

        // 修复
        public List<ChangeItem> fixList { get; set; }

        // 定制
        public List<ChangeItem> oemList { get; set; }

        public Changelog()
        {
            unkownList = new List<ChangeItem>();
            addList = new List<ChangeItem>();
            backList = new List<ChangeItem>();
            optimizeList = new List<ChangeItem>();
            fixList = new List<ChangeItem>();
            oemList = new List<ChangeItem>();
        }
    }
}
