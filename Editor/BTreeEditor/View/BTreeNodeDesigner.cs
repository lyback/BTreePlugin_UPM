﻿
using System.Collections.Generic;
using BTreeFrame;
using UnityEngine;
using System;

namespace BTree.Editor
{
    public class BTreeNodeDesigner
    {
        public BTreeEditorNode m_EditorNode;
        public BTreeNodeDesigner m_ParentNode;
        public List<BTreeNodeDesigner> m_ChildNodeList;
        public List<BTreeNodeConnection> m_ChildNodeConnectionList;
        public BTreeNodeConnection m_ParentNodeConnection;

        public string m_NodeName { get { return m_EditorNode.m_Node.Name; } }

        private int _Index = -1;
        private int m_Index
        {
            get
            {
                if (_Index == -1)
                {
                    if (m_ParentNode != null)
                    {
                        _Index = 0;
                        for (int i = 0; i < m_ParentNode.m_ChildNodeList.Count; i++)
                        {
                            if (m_ParentNode.m_ChildNodeList[i].Equals(this))
                            {
                                _Index = i + 1;
                                return _Index;
                            }
                        }
                    }
                }
                return _Index;
            }
        }
        private bool m_Selected;
        private bool m_IsDirty = true;
        private bool m_IsShowHoverBar;
        public bool m_IsEntryDisplay;
        public bool m_IsDisable
        {
            get
            {
                if (m_EditorNode != null)
                {
                    return m_EditorNode.m_Disable;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool m_IsParent
        {
            get
            {
                if (m_EditorNode != null)
                {
                    return m_EditorNode.m_Node.m_ChildCount != 0;
                }
                else
                {
                    return true;
                }
            }
        }

        private Texture m_Icon;

        public BTreeNodeDesigner(BTreeEditorNode _editorNode)
        {
            if (_editorNode == null)
            {
                UnityEngine.Debug.Log("BTreeNodeDesigner Init Null");
                return;
            }
            m_EditorNode = _editorNode;
            m_ChildNodeList = new List<BTreeNodeDesigner>();
            m_ChildNodeConnectionList = new List<BTreeNodeConnection>();
            loadTaskIcon();
        }
        #region 节点操作方法
        public void disable()
        {
            m_EditorNode.m_Disable = true;
        }
        public void enable()
        {
            m_EditorNode.m_Disable = false;
        }
        public void select()
        {
            m_Selected = true;
        }
        public void deselect()
        {
            m_Selected = false;
        }
        public void delectChildNode(BTreeNodeDesigner childNodeDesigner)
        {
            m_ChildNodeList.Remove(childNodeDesigner);
            for (int i = 0; i < m_ChildNodeList.Count; i++)
            {
                m_ChildNodeList[i].ReSetIndex();
            }
            for (int i = 0; i < m_ChildNodeConnectionList.Count; i++)
            {
                if (m_ChildNodeConnectionList[i].m_DestinationNodeDesigner.Equals(childNodeDesigner))
                {
                    m_ChildNodeConnectionList.RemoveAt(i);
                    m_EditorNode.DelChildNode(childNodeDesigner.m_EditorNode);
                    markDirty();
                    break;
                }
            }
            childNodeDesigner.m_ParentNode = null;
        }
        public void AddChildNode(BTreeNodeDesigner destNode)
        {
            BTreeNodeConnection line = new BTreeNodeConnection(destNode, this, NodeConnectionType.Outgoing);
            if (m_ChildNodeConnectionList == null)
            {
                m_ChildNodeConnectionList = new List<BTreeNodeConnection>();
            }
            for (int i = 0; i < m_ChildNodeConnectionList.Count; i++)
            {
                if (m_ChildNodeConnectionList[i].m_DestinationNodeDesigner.Equals(destNode))
                {
                    return;
                }
            }
            m_ChildNodeConnectionList.Add(line);
            m_ChildNodeList.Add(destNode);
            m_EditorNode.AddChildNode(destNode.m_EditorNode);
            destNode.AddParentConnectionLine(this);
            destNode.ReSetIndex();
            markDirty();
        }
        public void AddParentConnectionLine(BTreeNodeDesigner orgNode)
        {
            BTreeNodeConnection line = new BTreeNodeConnection(this, orgNode, NodeConnectionType.Incoming);
            if (m_ParentNodeConnection != null)
            {
                m_ParentNodeConnection.m_OriginatingNodeDesigner.delectChildNode(this);
            }
            m_ParentNodeConnection = line;
            m_ParentNode = orgNode;
            markDirty();
        }
        public void movePosition(Vector2 delta)
        {
            Vector2 vector = m_EditorNode.m_Pos;
            vector += delta;
            m_EditorNode.m_Pos = vector;
            if (m_ParentNode != null)
            {
                m_ParentNode.markDirty();
            }
            markDirty();
        }
        public void markDirty()
        {
            m_IsDirty = true;
        }
        public Rect IncomingConnectionRect(Vector2 offset)
        {
            Rect rect = rectangle(offset, false);
            return new Rect(rect.x + (rect.width - BTreeEditorUtility.ConnectionWidth) / 2f, rect.y - BTreeEditorUtility.TopConnectionHeight, BTreeEditorUtility.ConnectionWidth, BTreeEditorUtility.TopConnectionHeight);
        }
        public Rect OutgoingConnectionRect(Vector2 offset)
        {
            Rect rect = rectangle(offset, false);
            return new Rect(rect.x + (rect.width - BTreeEditorUtility.ConnectionWidth) / 2f, rect.yMax, BTreeEditorUtility.ConnectionWidth, BTreeEditorUtility.BottomConnectionHeight);
        }
        public void SetEntryDisplay(bool isEntry)
        {
            m_IsEntryDisplay = isEntry;
        }
        public void MoveUpIndex()
        {
            if (m_ParentNode != null)
            {
                for (int i = 0; i < m_ParentNode.m_ChildNodeList.Count; i++)
                {
                    if (m_ParentNode.m_ChildNodeList[i].Equals(this))
                    {
                        if (i > 0)
                        {
                            var temp = m_ParentNode.m_ChildNodeList[i - 1];
                            m_ParentNode.m_ChildNodeList[i - 1] = this;
                            m_ParentNode.m_ChildNodeList[i] = temp;
                            m_ParentNode.m_ChildNodeList[i].ReSetIndex();
                            m_ParentNode.m_ChildNodeList[i - 1].ReSetIndex();
                            break;
                        }
                    }
                }
            }
            m_EditorNode.MoveUpIndex();
        }
        public void MoveDownIndex()
        {
            if (m_ParentNode != null)
            {
                for (int i = 0; i < m_ParentNode.m_ChildNodeList.Count; i++)
                {
                    if (m_ParentNode.m_ChildNodeList[i].Equals(this))
                    {
                        if (i < m_ParentNode.m_ChildNodeList.Count - 1)
                        {
                            var temp = m_ParentNode.m_ChildNodeList[i + 1];
                            m_ParentNode.m_ChildNodeList[i + 1] = this;
                            m_ParentNode.m_ChildNodeList[i] = temp;
                            m_ParentNode.m_ChildNodeList[i].ReSetIndex();
                            m_ParentNode.m_ChildNodeList[i + 1].ReSetIndex();
                            break;
                        }
                    }
                }
            }
            m_EditorNode.MoveDownIndex();
        }
        public void MoveIndexToTop()
        {
            if (m_ParentNode != null && m_ParentNode.m_ChildNodeList.Count > 0)
            {
                for (int i = m_ParentNode.m_ChildNodeList.Count - 1; i > 0; i--)
                {
                    if (m_ParentNode.m_ChildNodeList[i].Equals(this))
                    {
                        if (i > 0)
                        {
                            var temp = m_ParentNode.m_ChildNodeList[i - 1];
                            m_ParentNode.m_ChildNodeList[i - 1] = this;
                            m_ParentNode.m_ChildNodeList[i] = temp;
                            m_ParentNode.m_ChildNodeList[i].ReSetIndex();
                            m_ParentNode.m_ChildNodeList[i - 1].ReSetIndex();
                        }
                    }
                }
            }
            m_EditorNode.MoveIndexToTop();
        }
        public void MoveIndexToEnd()
        {
            if (m_ParentNode != null)
            {
                for (int i = 0; i < m_ParentNode.m_ChildNodeList.Count; i++)
                {
                    if (m_ParentNode.m_ChildNodeList[i].Equals(this))
                    {
                        if (i < m_ParentNode.m_ChildNodeList.Count - 1)
                        {
                            var temp = m_ParentNode.m_ChildNodeList[i + 1];
                            m_ParentNode.m_ChildNodeList[i + 1] = this;
                            m_ParentNode.m_ChildNodeList[i] = temp;
                            m_ParentNode.m_ChildNodeList[i].ReSetIndex();
                            m_ParentNode.m_ChildNodeList[i + 1].ReSetIndex();
                        }
                    }
                }
            }
            m_EditorNode.MoveIndexToEnd();
        }
        public void ReSetIndex()
        {
            _Index = -1;
        }

        //断开节点
        public void DisconnectNode()
        {
            if (m_ParentNode != null)
            {
                if (m_ParentNodeConnection != null)
                {
                    m_ParentNodeConnection.m_OriginatingNodeDesigner.delectChildNode(this);
                }
                m_ParentNodeConnection = null;
                m_ParentNode = null;
                markDirty();
            }
        }

        #endregion
        #region 绘制方法相关
        //绘制节点
        public bool drawNode(Vector2 offset, bool drawSelected, bool disabled)
        {
            Rect rect = rectangle(offset, false);
            GUI.color = m_IsDisable ? new Color(0.7f, 0.7f, 0.7f) : Color.white;
            //上部
            if (!m_IsEntryDisplay)
            {
                GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.ConnectionWidth) / 2f, rect.y - BTreeEditorUtility.TopConnectionHeight - BTreeEditorUtility.TaskBackgroundShadowSize + 4f, BTreeEditorUtility.ConnectionWidth, (BTreeEditorUtility.TopConnectionHeight + BTreeEditorUtility.TaskBackgroundShadowSize)), BTreeEditorUtility.TaskConnectionTopTexture, ScaleMode.ScaleToFit);
            }
            //下部
            if (m_IsEntryDisplay || !m_EditorNode.m_Node.m_IsAcitonNode)
            {
                GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.ConnectionWidth) / 2f, rect.yMax - 3f, BTreeEditorUtility.ConnectionWidth, (BTreeEditorUtility.BottomConnectionHeight + BTreeEditorUtility.TaskBackgroundShadowSize)), BTreeEditorUtility.TaskConnectionBottomTexture, ScaleMode.ScaleToFit);
            }
            //背景
            GUI.Label(rect, "", m_Selected ? BTreeEditorUtility.TaskSelectedGUIStyle : BTreeEditorUtility.TaskGUIStyle);
            //图标背景
            GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.IconBorderSize) / 2f, rect.y + ((BTreeEditorUtility.IconAreaHeight - BTreeEditorUtility.IconBorderSize) / 2) + 2f, BTreeEditorUtility.IconBorderSize, BTreeEditorUtility.IconBorderSize), BTreeEditorUtility.TaskBorderTexture);
            //图标
            GUI.DrawTexture(new Rect(rect.x + (rect.width - BTreeEditorUtility.IconSize) / 2f, rect.y + ((BTreeEditorUtility.IconAreaHeight - BTreeEditorUtility.IconSize) / 2) + 2f, BTreeEditorUtility.IconSize, BTreeEditorUtility.IconSize), m_Icon);
            if (m_IsShowHoverBar)
            {
                GUI.DrawTexture(new Rect(rect.x - 1f, rect.y - 17f, 14f, 14f), m_EditorNode.m_Disable ? BTreeEditorUtility.EnableTaskTexture : BTreeEditorUtility.DisableTaskTexture, ScaleMode.ScaleToFit);
                if (m_IsParent)
                {
                    GUI.DrawTexture(new Rect(rect.x + 15f, rect.y - 17f, 14f, 14f), m_EditorNode.m_IsCollapsed ? BTreeEditorUtility.ExpandTaskTexture : BTreeEditorUtility.CollapseTaskTexture, ScaleMode.ScaleToFit);
                }
            }
            GUI.Label(new Rect(rect.x, rect.yMax - BTreeEditorUtility.TitleHeight - 1f, rect.width, BTreeEditorUtility.TitleHeight), ToString(), BTreeEditorUtility.TaskTitleGUIStyle);
            return true;
        }
        //绘制连线
        public void drawNodeConnection(Vector2 offset, float graphZoom, bool disabled)
        {
            if (m_IsDirty)
            {
                determineConnectionHorizontalHeight(rectangle(offset, false), offset);
                m_IsDirty = false;
            }
            if (m_IsParent)
            {
                for (int i = 0; i < m_ChildNodeConnectionList.Count; i++)
                {
                    m_ChildNodeConnectionList[i].drawConnection(offset, graphZoom, disabled);
                }
            }
        }
        //绘制节点说明
        public void drawNodeComment(Vector2 offset)
        {

        }
        //获取连线位置
        public Vector2 getConnectionPosition(Vector2 offset, NodeConnectionType connectionType)
        {
            Vector2 result;
            if (connectionType == NodeConnectionType.Incoming)
            {
                Rect rect = IncomingConnectionRect(offset);
                result = new Vector2(rect.center.x, rect.y + (BTreeEditorUtility.TopConnectionHeight / 2));
            }
            else
            {
                Rect rect2 = OutgoingConnectionRect(offset);
                result = new Vector2(rect2.center.x, rect2.yMax - (BTreeEditorUtility.BottomConnectionHeight / 2));
            }
            return result;
        }
        #endregion
        private void loadTaskIcon()
        {
            Texture2D _icon = null;
            if (m_EditorNode.m_Node.m_IsAcitonNode)
            {
                _icon = BTreeEditorUtility.LoadTexture("ActionIcon.png");
            }
            else
            {
                Type type = m_EditorNode.m_Node.GetType();
                if (type == typeof(BTreeNodePrioritySelector))
                {
                    _icon = BTreeEditorUtility.PrioritySelectorIcon;
                }
                else if (type == typeof(BTreeNodeNonePrioritySelector))
                {
                    _icon = BTreeEditorUtility.PrioritySelectorIcon;
                }
                else if (type == typeof(BTreeNodeSequence))
                {
                    _icon = BTreeEditorUtility.SequenceIcon;

                }
                else if (type == typeof(BTreeNodeSequence_EvaluateOnce))
                {
                    _icon = BTreeEditorUtility.SequenceIcon;

                }
                else if (type == typeof(BTreeNodeParalle))
                {
                    _icon = BTreeEditorUtility.ParallelSelectorIcon;
                }
                else
                {
                    _icon = BTreeEditorUtility.InverterIcon;
                }
            }
            m_Icon = _icon;
        }

        private Rect rectangle(Vector2 offset, bool includeConnections)
        {
            Rect result = rectangle(offset);
            if (includeConnections)
            {
                if (!m_IsEntryDisplay)
                {
                    result.yMin = (result.yMin - BTreeEditorUtility.TopConnectionHeight);
                }
                if (m_IsParent)
                {
                    result.yMax = (result.yMax + BTreeEditorUtility.BottomConnectionHeight);
                }
            }
            return result;
        }
        private Rect rectangle(Vector2 offset)
        {
            if (m_EditorNode == null)
            {
                return default(Rect);
            }
            float num = BTreeEditorUtility.TaskTitleGUIStyle.CalcSize(new GUIContent(ToString())).x + BTreeEditorUtility.TextPadding;
            if (!m_IsParent)
            {
                float num2;
                float num3;
                BTreeEditorUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent("Comment(Test)"), out num2, out num3);
                num3 += BTreeEditorUtility.TextPadding;
                num = ((num > num3) ? num : num3);
            }
            num = Mathf.Min(BTreeEditorUtility.MaxWidth, Mathf.Max(BTreeEditorUtility.MinWidth, num));
            return new Rect(m_EditorNode.m_Pos.x + offset.x - num / 2f, m_EditorNode.m_Pos.y + offset.y, num, (BTreeEditorUtility.IconAreaHeight + BTreeEditorUtility.TitleHeight));
        }
        //确定连线横向高度
        private void determineConnectionHorizontalHeight(Rect nodeRect, Vector2 offset)
        {
            if (m_IsParent)
            {
                float num = float.MaxValue;
                float num2 = num;
                for (int i = 0; i < m_ChildNodeConnectionList.Count; i++)
                {
                    Rect rect = m_ChildNodeConnectionList[i].m_DestinationNodeDesigner.rectangle(offset, false);
                    if (rect.y < num)
                    {
                        num = rect.y;
                        num2 = rect.y;
                    }
                }
                num = num * 0.75f + nodeRect.yMax * 0.25f;
                if (num < nodeRect.yMax + 15f)
                {
                    num = nodeRect.yMax + 15f;
                }
                else if (num > num2 - 15f)
                {
                    num = num2 - 15f;
                }
                for (int j = 0; j < m_ChildNodeConnectionList.Count; j++)
                {
                    m_ChildNodeConnectionList[j].m_HorizontalHeight = num;
                }
            }
        }
        //是否包含坐标
        public bool contains(Vector2 point, Vector2 offset, bool includeConnections)
        {
            return rectangle(offset, includeConnections).Contains(point);
        }

        public BTreeEditorNode Clone()
        {
            BTreeEditorNode _editorNode = new BTreeEditorNode(m_EditorNode.m_Node.Clone() as BTreeNode);
            return _editorNode;
        }

        public override string ToString()
        {
            string isEntry = m_IsEntryDisplay ? "(Entry)" : "";
            string index = m_ParentNode != null ? "(" + m_Index + ")" : "";
            if (m_NodeName == null)
            {
                return isEntry + index;
            }

            string name = m_NodeName.Replace("BTreeNode", "");
            return name + isEntry + index;
        }
    }
}
