using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace RomVersionTools.Tools
{
    class DataTools
    {
        private ModelTool mModelT = null;
        private VersionTool mVersionT = null;

        private NodeComparer mNodeCompare = null;

        public DataTools()
        {
            mModelT = new ModelTool();
            mVersionT = new VersionTool();

            mNodeCompare = new NodeComparer();
        }

        ~DataTools()
        { 
        }

        public List<VersionNodeItem> LoadDatas(CancellationToken ct, MainWindow win, string strFolderPath)
        {
            if (null == ct)
            {
                return null;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(strFolderPath);
            if (null == dirInfo)
            {
                return null;
            }

            FileInfo[] fileInfos = dirInfo.GetFiles("*.cfg");
            if (null == fileInfos || 0 >= fileInfos.Length)
            {
                return null;
            }

            List<VersionNodeItem> list = new List<VersionNodeItem>();
            list.Clear();

            VersionNodeItem curNode = null;
            foreach (FileInfo fileInfo in fileInfos)
            {
                ct.ThrowIfCancellationRequested();

                curNode = ReadAndParseNode(win, fileInfo);
                if (null != curNode)
                {
                    list = Insert2List(win, list, curNode);
                }
            }

            return list;
        }

        public bool SaveDatas(CancellationToken ct, MainWindow win, List<VersionNodeItem> dataTree)
        {
            return ScanAndWrite(ct, dataTree, null);
        }

        public bool IsValidNodeName(VersionNodeItem node)
        {
            try
            {
                return mVersionT.IsValidVersion(
                    node.nodeName.Split('.'), node.version.Split('.'));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ResetNodeName(VersionNodeItem node)
        {
            if (null == node)
            {
                return;
            }

            node.nodeName = node.version;
        }

        public void ModifiedNodeName(VersionNodeItem node)
        {
            if (null == node)
            {
                return;
            }

            if (0 == string.Compare(node.nodeName, node.version))
            {
                return;
            }

            node.version = node.nodeName;
            node.modified = true;
        }

        private VersionNodeItem ReadAndParseNode(MainWindow win, FileInfo fileInfo)
        {
            if (null == fileInfo)
            {
                return null;
            }

            StreamReader streamIn = null;
            try 
            {
                streamIn = fileInfo.OpenText();
                VersionNodeItem node = Parse(streamIn);
                if (null == node)
                {
                    // Bad parse
                    return null;
                }

                // 叶子节点都是可以编辑的
                node.nodeName = node.version;
                if (null != win)
                {
                    node.desc = win.FindResource("file_name") + ": " + fileInfo.Name
                        + Environment.NewLine
                        + win.FindResource("oem") + ": " + node.oem;
                }
                node.editable = true;
                node.fileInfo = fileInfo;
                node.child = new List<VersionNodeItem>();
                return node;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (null != streamIn)
                {
                    streamIn.Close();
                    streamIn.Dispose();
                    streamIn = null;
                }
            }
        }

        private VersionNodeItem Parse(StreamReader streamIn)
        {
            if (null == streamIn)
            {
                return null;
            }

            VersionNodeItem node = new VersionNodeItem();
            string line = null;
            while (null != (line = streamIn.ReadLine()))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (line.Contains("#define MOD_NAME"))
                {
                    node.modName = line.Substring(line.IndexOf('"') + 1,
                        line.Length - line.IndexOf('"') - 2/* 2个引号 */);
                }
                else if (line.Contains("#define OEM_NAME"))
                {
                    node.company = line.Substring(line.IndexOf('"') + 1,
                        line.Length - line.IndexOf('"') - 2/* 2个引号 */);
                }
                else if (line.Contains("#define TARGET_OEM_NAME"))
                {
                    node.oem = line.Substring(line.IndexOf('"') + 1,
                        line.Length - line.IndexOf('"') - 2/* 2个引号 */);
                }
                else if (line.Contains("#define MOD_REVISION"))
                {
                    node.version = line.Substring(line.IndexOf('"') + 1,
                        line.Length - line.IndexOf('"') - 2/* 2个引号 */);
                    string[] versionS = node.version.Split('.');
                    node.versionI = new int[versionS.Length];

                    int i = 0;
                    foreach (string v in versionS)
                    {
                        node.versionI[i++] = int.Parse(v);
                    }
                }
            }

            return node;
        }

        private List<VersionNodeItem> Insert2List(MainWindow win, List<VersionNodeItem> list, VersionNodeItem node) 
        {
            /************************************************************************/
            /* 模糊机型层                                                           */
            /************************************************************************/
            List<VersionNodeItem> curLevelList = mModelT.GetModelXLevel(win, list, node);

            /************************************************************************/
            /* 一级版本层                                                           */
            /************************************************************************/
            curLevelList = mVersionT.GetVersionLevel1(win, curLevelList, node);

            /************************************************************************/
            /* 二级版本层                                                           */
            /************************************************************************/
            curLevelList = mVersionT.GetVersionLevel2(win, curLevelList, node);

            /************************************************************************/
            /* 精确机型层                                                           */
            /************************************************************************/
            curLevelList = mModelT.GetModelLevel(win, curLevelList, node);

            /************************************************************************/
            /* 同版本层                                                             */
            /************************************************************************/
            curLevelList = mVersionT.GetSameVersionLevel(win, curLevelList, node);

            /* 添加节点 */
            curLevelList.Add(node);
            curLevelList.Sort(mNodeCompare);
            return list;
        }

        private bool ScanAndWrite(CancellationToken ct, List<VersionNodeItem> dataTree, VersionNodeItem parent)
        {
            if (null == ct || null == dataTree)
            {
                return false;
            }

            bool parentModified = false;
            if (null != parent)
            {
                parentModified = parent.modified;
            }

            foreach (VersionNodeItem curNode in dataTree)
            {
                ct.ThrowIfCancellationRequested();

                if (null == curNode)
                {
                    continue;
                }

                if (parentModified)
                {
                    // 父节点有编辑, 则对应子节点列表所有都要编辑
                    Write(curNode.fileInfo, parent.version, curNode.versionI);
                }
                else if (curNode.modified)
                {
                    // 子节点本身有编辑
                    Write(curNode.fileInfo, curNode.version, curNode.versionI);
                }

                // 递归
                ScanAndWrite(ct, curNode.child, curNode);
            }

            return true;
        }

        private void Write(FileInfo fileInfo, string modifiedVersion, int[] oriVersionI)
        {
            if (null == fileInfo || null == modifiedVersion || null == oriVersionI)
            {
                return;
            }

            try
            {
                string[] modifiedVersionS = modifiedVersion.Split('.');
                if (modifiedVersionS.Length != oriVersionI.Length)
                {
                    return;
                }

                string dstVersion = oriVersionI[0] + "." +  oriVersionI[1] + "."
                    + modifiedVersionS[2] + "." + modifiedVersionS[3];
                File.WriteAllText(fileInfo.FullName, 
                    Regex.Replace(File.ReadAllText(fileInfo.FullName),
                    "#define MOD_REVISION .*",
                    "#define MOD_REVISION " + '"' + dstVersion + '"'));
            }
            catch (Exception)
            {
                return;
            }
        }

        private class NodeComparer : IComparer<VersionNodeItem>
        {
            public int Compare(VersionNodeItem node1, VersionNodeItem node2)
            {
                for (int i = 0; i < node1.versionI.Length; ++i)
                {
                    if (node1.versionI[i] > node2.versionI[i])
                    {
                        return 1;
                    }
                    else if (node1.versionI[i] < node2.versionI[i])
                    {
                        return -1;
                    }
                }

                return 0;
            }
        }
    } 
}
