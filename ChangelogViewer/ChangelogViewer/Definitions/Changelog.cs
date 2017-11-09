using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangelogViewer.Definitions
{
    public class ChangeRecord
    {
        public string change { set; get; }
        public bool tested { set; get; }

        public ChangeRecord(string change)
        {
            this.change = change;
            this.tested = false;
        }
    }

    public class Changelog
    {
        public string oldVersionStr;        // 旧版本号
        public int[] oldVersionInt;         // -1表示x

        public string newVersionStr;        // 新版本号
        public int[] newVersionInt;         // -1表示x

        public List<string> deviceList;     // 兼容平台&设备列表

        public List<string> oemList;        // oem列表

        public List<ChangeRecord> changeAddList;  // 新增列表
        public List<ChangeRecord> changeFixList;  // 修复列表
        public List<ChangeRecord> changeOptList;  // 优化列表
        public List<ChangeRecord> changeOemList;  // 定制列表
        public List<ChangeRecord> changeOthList;// 其他列表, 用于记录不规范书写的修改

        public string svnAddr;              // SVN地址
        public string svnBranch;            // SVN分支
        public string svnRevision;          // SVN版本

        public string gitAddr;              // git地址
        public string gitBranch;            // git分支
        public string gitRevision;          // git版本

        public string date;                 // 发布日期

        public string desc;                 // 描述

        public Changelog()
        {
        }

        public Changelog(Changelog startChangelog, Changelog endChangelog)
        {
            this.oldVersionStr = startChangelog.newVersionStr;
            this.oldVersionInt = startChangelog.newVersionInt;

            this.newVersionStr = endChangelog.newVersionStr;
            this.newVersionInt = endChangelog.newVersionInt;

            this.deviceList = endChangelog.deviceList;  // 设备列表要以最新版本为准
            this.oemList = endChangelog.oemList;        // oem列表要以最新版本为准
            
            /* changelog list相关的仅做初始化 */
            this.changeAddList = new List<ChangeRecord>();
            this.changeFixList = new List<ChangeRecord>();
            this.changeOptList = new List<ChangeRecord>();
            this.changeOemList = new List<ChangeRecord>();
            this.changeOthList = new List<ChangeRecord>();

            this.svnAddr = endChangelog.svnAddr;
            this.svnBranch = endChangelog.svnBranch;
            this.svnRevision = endChangelog.svnRevision;

            this.gitAddr = endChangelog.gitAddr;
            this.gitBranch = endChangelog.gitBranch;
            this.gitRevision = endChangelog.gitRevision;

            this.date = endChangelog.date;

            this.desc = endChangelog.desc;
        }
    }
}
