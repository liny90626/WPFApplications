using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartChangelog.Definitions
{
    public class Constant
    {
        public enum UiState
        {
            Idle,
            IdleReady,
            Loading,
            QuestionAndAnswer,
            Reporting,
            Report,
        }

        public enum LogType
        {
            Svn,
            Git,
        }

        public enum ChangeType
        {
            Unknown,    // 未知
            Add,        // 新增
            Back,       // 回退
            Optimize,   // 优化
            Fix,        // 修复
            Oem,        // 定制
        }

        public enum DictName
        {
            SvnBranchName,
            SvnLastVersion,
            SvnCurrentVersion,
            GitRepositoryName,
            GitBranchName,
            GitLastVersion,
            GitCurrentVersion,
        }

        public class Cfg 
        {
            public const string Name = "AppSettings";

            public const string SvnServerAddr = "SvnServerAddr";
            public const string GitServerAddr = "GitServerAddr";

            public const string SvnRegxEventId = "SvnRegxEventId";
            public const string SvnRegxContent = "SvnRegxContent";

            public const string SvnRegxAdd = "SvnRegxAdd";
            public const string SvnRegxBack = "SvnRegxBack";
            public const string SvnRegxOptimize = "SvnRegxOptimize";
            public const string SvnRegxFix = "SvnRegxFix";
            public const string SvnRegxOem = "SvnRegxOem";
        }
    }
}
