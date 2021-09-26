  
using BTreeFrame;
using System;
using System.Collections.Generic;

namespace BTree.Editor
{
    [Serializable]
    public class BTreeEditorConfig
    {
        public BTreeEditorNodeConfig m_EnterTreeNode;
        public List<BTreeEditorNodeConfig> m_DetachedTreeNode;
        public BTreeEditorConfig()
        {
            m_DetachedTreeNode = new List<BTreeEditorNodeConfig>();
        }
    }
    [Serializable]
    public class BTreeEditorNodeConfig{
        public string m_RootNodeJson;
        public List<BTreeEditorNodeConfig> m_ChildNodes;
        
        public BTreeEditorNodeConfig()
        {
            m_ChildNodes = new List<BTreeEditorNodeConfig>();
        }
    }
}
