using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RomVersionTools.Tools
{
    class VersionTool
    {
        private const int VERSION_LENGTH = 4;
        private const int VERSION_LEVEL_MODEL_INDEX = 0;
        private const int VERSION_LEVEL_OEM_INDEX = 1;
        private const int VERSION_LEVEL_1_INDEX = 2;
        private const int VERSION_LEVEL_2_INDEX = 3;

        private VersionLevel1Comparer mVersionLevel1Compare = null;
        private VersionLevel2Comparer mVersionLevel2Compare = null;

        public VersionTool()
        {
            mVersionLevel1Compare = new VersionLevel1Comparer();
            mVersionLevel2Compare = new VersionLevel2Comparer();
        }

        public List<VersionNodeItem> GetVersionLevel1(MainWindow win, List<VersionNodeItem> list, VersionNodeItem node)
        {
            if (null == list || null == node)
            {
                return list;
            }

            List<VersionNodeItem> curLevelList = list;
            foreach (VersionNodeItem curNode in curLevelList)
            {
                if (null != curNode && IsSameVersionLevel1(curNode.versionI, node.versionI))
                {
                    // 跳到下一级
                    return curNode.child;
                }
            }

            // 找不到, 新增一级
            VersionNodeItem newNode = new VersionNodeItem();
            newNode.versionI = new int[node.versionI.Length];
            newNode.versionI[VERSION_LEVEL_MODEL_INDEX] = 0;
            newNode.versionI[VERSION_LEVEL_OEM_INDEX] = 0;
            newNode.versionI[VERSION_LEVEL_1_INDEX] =
                node.versionI[VERSION_LEVEL_1_INDEX];
            newNode.versionI[VERSION_LEVEL_2_INDEX] = 0;

            newNode.nodeName = "V" + newNode.versionI[VERSION_LEVEL_1_INDEX];
            if (null != win)
            {
                newNode.desc = (string)win.FindResource("desc_version_level_1");
            }
            newNode.child = new List<VersionNodeItem>();

            list.Add(newNode);
            list.Sort(mVersionLevel1Compare);
            return newNode.child;
        }

        public List<VersionNodeItem> GetVersionLevel2(MainWindow win, List<VersionNodeItem> list, VersionNodeItem node)
        {
            if (null == list || null == node)
            {
                return list;
            }

            List<VersionNodeItem> curLevelList = list;
            foreach (VersionNodeItem curNode in curLevelList)
            {
                if (null != curNode 
                    && IsSameVersionLevel1(curNode.versionI, node.versionI)
                    && IsSameVersionLevel2(curNode.versionI, node.versionI))
                {
                    // 跳到下一级
                    return curNode.child;
                }
            }

            // 找不到, 新增一级
            VersionNodeItem newNode = new VersionNodeItem();
            newNode.versionI = new int[node.versionI.Length];
            newNode.versionI[VERSION_LEVEL_MODEL_INDEX] = 0;
            newNode.versionI[VERSION_LEVEL_OEM_INDEX] = 0;
            newNode.versionI[VERSION_LEVEL_1_INDEX] =
                node.versionI[VERSION_LEVEL_1_INDEX];
            newNode.versionI[VERSION_LEVEL_2_INDEX] =
                node.versionI[VERSION_LEVEL_2_INDEX] 
                - (node.versionI[VERSION_LEVEL_2_INDEX] % 100);
 
            newNode.nodeName = "V" + newNode.versionI[VERSION_LEVEL_1_INDEX]
                + "." + (newNode.versionI[VERSION_LEVEL_2_INDEX] / 100);
            if (null != win)
            {
                newNode.desc = (string)win.FindResource("desc_version_level_2");
            }
            newNode.child = new List<VersionNodeItem>();

            list.Add(newNode);
            list.Sort(mVersionLevel2Compare);
            return newNode.child;
        }

        public List<VersionNodeItem> GetSameVersionLevel(MainWindow win, List<VersionNodeItem> list, VersionNodeItem node)
        {
            if (null == list || null == node)
            {
                return list;
            }

            List<VersionNodeItem> curLevelList = list;
            foreach (VersionNodeItem curNode in curLevelList)
            {
                if (null != curNode
                    && IsSameVersion(curNode.versionI, node.versionI))
                {
                    // 跳到下一级
                    return curNode.child;
                }
            }

            // 找不到, 新增一级
            VersionNodeItem newNode = new VersionNodeItem();
            newNode.versionI = new int[node.versionI.Length];
            newNode.versionI[VERSION_LEVEL_MODEL_INDEX] = 
                node.versionI[VERSION_LEVEL_MODEL_INDEX];
            newNode.versionI[VERSION_LEVEL_OEM_INDEX] = 0;
            newNode.versionI[VERSION_LEVEL_1_INDEX] =
                node.versionI[VERSION_LEVEL_1_INDEX];
            newNode.versionI[VERSION_LEVEL_2_INDEX] =
                node.versionI[VERSION_LEVEL_2_INDEX];

            newNode.version = newNode.versionI[VERSION_LEVEL_MODEL_INDEX]
                + ".x." + newNode.versionI[VERSION_LEVEL_1_INDEX]
                + "." + newNode.versionI[VERSION_LEVEL_2_INDEX];
            newNode.nodeName = newNode.version;
            if (null != win)
            {
                newNode.desc = (string)win.FindResource("desc_same_version_level");
            }
            newNode.editable = true;
            newNode.child = new List<VersionNodeItem>();

            list.Add(newNode);
            list.Sort(mVersionLevel2Compare);
            return newNode.child;
        }

        public bool IsValidVersion(string[] nodeNameS, string[] nodeVersionS)
        {
            if (null == nodeNameS || null == nodeVersionS)
            {
                return false;
            }

            if (nodeNameS.Length != nodeVersionS.Length)
            {
                return false;
            }

            
            try
            {
                if (0 != string.Compare(nodeNameS[VERSION_LEVEL_MODEL_INDEX], 
                    nodeVersionS[VERSION_LEVEL_MODEL_INDEX], true))
                {
                    return false;
                }

                if (0 != string.Compare(nodeNameS[VERSION_LEVEL_OEM_INDEX],
                    nodeVersionS[VERSION_LEVEL_OEM_INDEX], true))
                {
                    return false;
                }

                int tmpV = int.Parse(nodeNameS[VERSION_LEVEL_1_INDEX]);
                if (1000 <= tmpV || 0 > tmpV)
                {
                    return false;
                }

                tmpV = int.Parse(nodeNameS[VERSION_LEVEL_2_INDEX]);
                if (10000 <= tmpV || 0 > tmpV)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool IsSameVersionLevel1(int[] versionI1, int[] versionI2)
        {
            if (null == versionI1 || versionI1.Length != VERSION_LENGTH
                || null == versionI2 || versionI2.Length != VERSION_LENGTH)
            {
                return false;
            }

            return (versionI1[VERSION_LEVEL_1_INDEX] == versionI2[VERSION_LEVEL_1_INDEX]);
        }

        private bool IsSameVersionLevel2(int[] versionI1, int[] versionI2)
        {
            if (null == versionI1 || versionI1.Length != VERSION_LENGTH
                || null == versionI2 || versionI2.Length != VERSION_LENGTH)
            {
                return false;
            }

            return ((versionI1[VERSION_LEVEL_2_INDEX] / 100) 
                == (versionI2[VERSION_LEVEL_2_INDEX] / 100));
        }

        private bool IsSameVersion(int[] versionI1, int[] versionI2)
        {
            if (null == versionI1 || versionI1.Length != VERSION_LENGTH
                || null == versionI2 || versionI2.Length != VERSION_LENGTH)
            {
                return false;
            }

            if (versionI1[VERSION_LEVEL_MODEL_INDEX]
                != versionI2[VERSION_LEVEL_MODEL_INDEX])
            {
                return false;
            }

            for (int i = VERSION_LEVEL_1_INDEX; i < VERSION_LENGTH; ++i)
            {
                if (versionI1[i] != versionI2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private class VersionLevel1Comparer : IComparer<VersionNodeItem>
        {
            public int Compare(VersionNodeItem node1, VersionNodeItem node2)
            {
                return node1.versionI[VERSION_LEVEL_1_INDEX].CompareTo(node2.versionI[VERSION_LEVEL_1_INDEX]);
            }
        }

        private class VersionLevel2Comparer : IComparer<VersionNodeItem>
        {
            public int Compare(VersionNodeItem node1, VersionNodeItem node2)
            {
                return node1.versionI[VERSION_LEVEL_2_INDEX].CompareTo(node2.versionI[VERSION_LEVEL_2_INDEX]);
            }
        }
    }
}
