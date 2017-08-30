using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RomVersionTools.Tools
{
    class ModelTool
    {
        private const string MODEL_NAME_UNKNOWN = "Unknown";

        private const string MODEL_NAME_IT80 = "IT80";
        private const string MODEL_NAME_IT82 = "IT82";
        private const string MODEL_NAME_IT83 = "IT83";
        private const string MODEL_NAME_IT8X = "IT8X";

        private const string MODEL_NAME_R47 = "R47";
        private const string MODEL_NAME_R48 = "R48";
        private const string MODEL_NAME_R49 = "R49";
        private const string MODEL_NAME_R4X = "R4X";

        public List<VersionNodeItem> GetModelXLevel(MainWindow win, List<VersionNodeItem> list, VersionNodeItem node)
        {
            if (null == list || null == node)
            {
                return list;
            }

            List<VersionNodeItem> curLevelList = list;
            foreach (VersionNodeItem curNode in curLevelList)
            {
                if (null != curNode && IsSameModelX(curNode.modName, node.modName))
                {
                    // 跳到下一级
                    return curNode.child;
                }
            }

            // 找不到, 新增一级
            VersionNodeItem newNode = new VersionNodeItem();
            if (IsIT8XModel(node.modName))
            {
                newNode.modName = MODEL_NAME_IT8X;
            }
            else if (IsR4XModel(node.modName))
            {
                newNode.modName = MODEL_NAME_R4X;
            }
            else
            {
                newNode.modName = MODEL_NAME_UNKNOWN;
            }

            newNode.nodeName = newNode.modName;
            if (null != win)
            {
                newNode.desc = (string)win.FindResource("desc_mod_x_level");
            }
            newNode.child = new List<VersionNodeItem>();

            list.Add(newNode);
            return newNode.child;
        }

        public List<VersionNodeItem> GetModelLevel(MainWindow win, List<VersionNodeItem> list, VersionNodeItem node)
        {
            if (null == list || null == node)
            {
                return list;
            }

            List<VersionNodeItem> curLevelList = list;
            foreach (VersionNodeItem curNode in curLevelList)
            {
                if (null != curNode && IsSameModel(curNode.modName, node.modName))
                {
                    // 跳到下一级
                    return curNode.child;
                }
            }

            // 找不到, 新增一级
            VersionNodeItem newNode = new VersionNodeItem();
            newNode.modName = node.modName.ToUpper();

            newNode.nodeName = newNode.modName;
            if (null != win)
            {
                newNode.desc = (string)win.FindResource("desc_mod_level");
            }
            newNode.child = new List<VersionNodeItem>();

            list.Add(newNode);
            return newNode.child;
        }

        private bool IsSameModelX(string modName1, string modName2)
        {
            // 模糊匹配
            if (IsIT8XModel(modName1)
                && IsIT8XModel(modName2))
            {
                return true;
            }

            if (IsR4XModel(modName1)
                && IsR4XModel(modName2))
            {
                return true;
            }

            if (IsSameModel(modName1, modName2))
            {
                return true;
            }

            return false;
        }

        private bool IsSameModel(string modName1, string modName2)
        {
            // 精确匹配
            if (string.IsNullOrWhiteSpace(modName1)
                || string.IsNullOrWhiteSpace(modName2))
            {
                return false;
            }

            return (0 == string.Compare(modName1, modName2, true));
        }

        private bool IsIT8XModel(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName))
            {
                return false;
            }

            if (0 == string.Compare(MODEL_NAME_IT80, modName, true)
                || 0 == string.Compare(MODEL_NAME_IT82, modName, true)
                || 0 == string.Compare(MODEL_NAME_IT83, modName, true)
                || 0 == string.Compare(MODEL_NAME_IT8X, modName, true))
            {
                return true;
            }

            return false;
        }

        private bool IsR4XModel(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName))
            {
                return false;
            }

            if (0 == string.Compare(MODEL_NAME_R47, modName, true)
                || 0 == string.Compare(MODEL_NAME_R48, modName, true)
                || 0 == string.Compare(MODEL_NAME_R49, modName, true)
                || 0 == string.Compare(MODEL_NAME_R4X, modName, true))
            {
                return true;
            }

            return false;
        }
    }
}
