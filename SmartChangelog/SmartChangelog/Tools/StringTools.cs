using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartChangelog.Tools
{
    public class StringTools
    {
        private const string REGX_INDEX = "(?:\\(|（){0,1}[0-9]{1,2}(?:,|}|\\)|）|、|\\.[\\s]+)";

        public static string TrimString(string str)
        {
            return Regex.Replace(str, "[\\*|#]", "").Trim();
        }

        public static string TrimStringWithMergeInfo(string str)
        {
            string tmp = Regex.Replace(str, "(?:CommitType).*[\\-]{64,}", "", 
                RegexOptions.Singleline | RegexOptions.IgnoreCase).Trim();
            return tmp;
        }

        public static string TrimStringWithSpace(string str)
        {
            return Regex.Replace(str, "[\\s|\\*|#]", "").Trim();
        }

        public static string TrimStringWithIndex(string str)
        {
            return Regex.Replace(str, REGX_INDEX, "").Trim();
        }

        public static string[] GetSubStringWithIndex(string str)
        {
            return Regex.Split(str, REGX_INDEX);
        }

        public static string TrimStringWithEventId(string str, string eventId)
        {
            string number = Regex.Replace(eventId, "[^0-9]", "").Trim();
            return Regex.Replace(str, "[A-Za-z]{0,}[\\s]{0,}" + number + "[\\s]{0,}", "").Trim();
        }
    }
}
