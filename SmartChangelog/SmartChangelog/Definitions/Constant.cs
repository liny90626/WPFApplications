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
            Learning,
            Learned,
        }

        public enum LogType
        {
            Svn,
            Git,
        }

        public enum ChangeType
        {
            Unknown = 0,    // 未知
            Add,            // 新增
            Back,           // 回退
            Optimize,       // 优化
            Fix,            // 修复
            Oem,            // 定制
            Max = 10,       // 最大值, 预留10种类型
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

        public class Neuro
        {
            // 神经网络允许的绝对误差
            public const double NeuronsError = 0.1;

            // 神经网络允许的最大学习次数, 避免未收敛
            public const long NeuronsMaxIterations = 10000;

            // 神经网络输入维度, content长度假设最长为10240个字节
            public const int NeuronsInputDimensions = 10240;

            // 神经网络输出维度 - Level 1, 输入是一维矢量
            public const int NeuronsOutputLevelFirst = 1;

            // 神经网络输出维度 - Level Middle
            public const int NeuronsOutputLevelMiddle = 8;
        }

        public class Cfg 
        {
            public const string Name = "appSettings";

            // 基础设置
            public const string SvnServerAddr = "SvnServerAddr";
            public const string GitServerAddr = "GitServerAddr";

            // Svn正则
            public const string SvnRegxEventId = "SvnRegxEventId";
            public const string SvnRegxContent = "SvnRegxContent";

            public const string SvnRegxAdd = "SvnRegxAdd";
            public const string SvnRegxBack = "SvnRegxBack";
            public const string SvnRegxOptimize = "SvnRegxOptimize";
            public const string SvnRegxFix = "SvnRegxFix";
            public const string SvnRegxOem = "SvnRegxOem";

            // 实验室
            public const string EnableRegx = "EnableRegx";
            public const string EnableAiLearning = "EnableAiLearning";
            public const string EnableAiDecisionChangeType = "EnableAiDecisionChangeType";

            // 神经网络统计
            public const string StatisticsSvnChangeTypeCnt = "StatisticsSvnChangeTypeCnt";
            public const string StatisticsSvnChangeTypeAccuracy = "StatisticsSvnChangeTypeAccuracy";
        }
    }
}
