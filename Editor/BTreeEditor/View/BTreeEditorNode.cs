
using BTreeFrame;
using UnityEngine;
using System.Collections.Generic;

namespace BTree.Editor
{
    public class BTreeEditorNode
    {
        public BTreeNode m_Node;
        public Vector2 m_Pos;
        public bool m_Disable;
        public bool m_IsCollapsed;//损坏
        public BTreeEditorNode()
        {

        }
        public BTreeEditorNode(BTreeNode _node)
        {
            m_Node = _node;
        }
        public BTreeEditorNode(BTreeNode _node, List<BTreeEditorNode> _childList)
        {
            m_Node = _node;
        }
        public void AddChildNode(BTreeEditorNode _node)
        {
            m_Node.AddChildNode(_node.m_Node);
        }
        public void DelChildNode(BTreeEditorNode _node)
        {
            m_Node.DelChildNode(_node.m_Node);
        }
        public void MoveUpIndex()
        {
            if (m_Node.m_ParentNode != null)
            {
                var childNodes = m_Node.m_ParentNode.m_ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    if (childNodes[i].Equals(m_Node))
                    {
                        if (i > 0)
                        {
                            var temp = childNodes[i - 1];
                            childNodes[i - 1] = m_Node;
                            childNodes[i] = temp;
                            break;
                        }
                    }
                }
            }
        }
        public void MoveDownIndex()
        {
            if (m_Node.m_ParentNode != null)
            {
                var childNodes = m_Node.m_ParentNode.m_ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    if (childNodes[i].Equals(m_Node))
                    {
                        if (i < childNodes.Count - 1)
                        {
                            var temp = childNodes[i + 1];
                            childNodes[i + 1] = m_Node;
                            childNodes[i] = temp;
                            break;
                        }
                    }
                }
            }
        }

        public void MoveIndexToTop()
        {
            if (m_Node.m_ParentNode != null && m_Node.m_ParentNode.m_ChildNodes.Count > 0)
            {
                var childNodes = m_Node.m_ParentNode.m_ChildNodes;
                for (int i = childNodes.Count-1; i > 0; i--)
                {
                    if (childNodes[i].Equals(m_Node))
                    {
                        if (i > 0)
                        {
                            var temp = childNodes[i - 1];
                            childNodes[i - 1] = m_Node;
                            childNodes[i] = temp;
                        }
                    }
                }
            }
        }
        public void MoveIndexToEnd()
        {
            if (m_Node.m_ParentNode != null)
            {
                var childNodes = m_Node.m_ParentNode.m_ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    if (childNodes[i].Equals(m_Node))
                    {
                        if (i < childNodes.Count - 1)
                        {
                            var temp = childNodes[i + 1];
                            childNodes[i + 1] = m_Node;
                            childNodes[i] = temp;
                        }
                    }
                }
            }
        }
    }
}
