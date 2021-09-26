
using UnityEngine;
using BTreeFrame;
using System;
using System.Text;
using System.Reflection;

namespace BTree.Editor
{
    class BTreeEditorGenLua
    {
        static string TAB = "\t";
        public static string GenLuaFromBTreeNode(BTreeNodeDesigner _dRoot)
        {
            BTreeNode _root = _dRoot.m_EditorNode.m_Node;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("local {0} = ", _root.Name);
            BTreeNode_TO_Lua(_dRoot, ref stringBuilder, "\n", true);
            stringBuilder.Append("return " + _root.Name);
            return stringBuilder.ToString();
        }
        //节点ToLua
        private static void BTreeNode_TO_Lua(BTreeNodeDesigner _dRoot, ref StringBuilder _sb, string _tab, bool first)
        {
            BTreeNode _root = _dRoot.m_EditorNode.m_Node;
            if (!_dRoot.m_EditorNode.m_Disable)
            {
                _sb.AppendFormat("{{{0}cls = \"{1}\",", _tab + TAB, GetBTreeNodeTypeName(_root));
                if (_dRoot.m_ChildNodeList.Count > 0)
                {
                    bool haveEnable = false;
                    for (int i = 0; i < _dRoot.m_ChildNodeList.Count; i++)
                    {
                        if (!_dRoot.m_ChildNodeList[i].m_EditorNode.m_Disable)
                        {
                            if (!haveEnable) _sb.Append(_tab + TAB + "childs = {" + _tab + TAB + TAB);
                            haveEnable = true;
                            BTreeNode_TO_Lua(_dRoot.m_ChildNodeList[i], ref _sb, _tab + TAB + TAB, false);
                        }
                    }
                    if (haveEnable) _sb.Append(_tab + TAB + "},");
                }
                if (_root.m_NodePrecondition != null)
                {
                    _sb.Append(_tab + TAB + "precondition = ");
                    Precondition_TO_Lua(_root.m_NodePrecondition, ref _sb, _tab + TAB);
                }
                GetClassParam(_root, ref _sb, _tab + TAB);
                if (first)
                {
                    _sb.Append(_tab + "}" + _tab);
                }
                else{
                    _sb.Append(_tab + "}," + _tab);
                }
            } 
        }
        //条件ToLua
        private static void Precondition_TO_Lua(BTreeNodePrecondition _condition, ref StringBuilder _sb, string _tab)
        {
            _sb.AppendFormat("{{{0}cls = \"{1}\",", _tab + TAB, GetBTreeNodePreconditionTypeName(_condition));
            Type type = _condition.GetType();
            if (type.Equals(typeof(BTreeNodePreconditionAND)))
            {
                var _cond = _condition as BTreeNodePreconditionAND;
                var _child = _cond.GetChildPrecondition();
                if (_child.Length > 0)
                {
                    _sb.Append(_tab + TAB + "childs = {" + _tab + TAB + TAB);
                    for (int i = 0; i < _child.Length; i++)
                    {
                        Precondition_TO_Lua(_child[i], ref _sb, _tab + TAB + TAB);
                    }
                    _sb.Append(_tab + TAB + "},");
                }
            }
            else if (type.Equals(typeof(BTreeNodePreconditionOR)))
            {
                var _cond = _condition as BTreeNodePreconditionOR;
                var _child = _cond.GetChildPrecondition();
                if (_child.Length > 0)
                {
                    _sb.Append(_tab + TAB + "childs = {" + _tab + TAB + TAB);
                    for (int i = 0; i < _child.Length; i++)
                    {
                        Precondition_TO_Lua(_child[i], ref _sb, _tab + TAB + TAB);
                    }
                    _sb.Append(_tab + TAB + "},");
                }
            }
            else if (type.Equals(typeof(BTreeNodePreconditionNOT)))
            {
                var _cond = _condition as BTreeNodePreconditionNOT;
                var _child = _cond.GetChildPrecondition();
                if (_child != null)
                {
                    _sb.Append(_tab + TAB + "childs = {" + _tab + TAB + TAB);
                    Precondition_TO_Lua(_child, ref _sb, _tab + TAB + TAB);
                    _sb.Append(_tab + TAB + "},");
                }
            }
            else
            {
                GetClassParam(_condition, ref _sb, _tab + TAB);
            }

            _sb.Append(_tab + "},");
        }
        private static void GetClassParam(object _obj, ref StringBuilder _sb, string _tab)
        {
            Type _Type = _obj.GetType();
            FieldInfo[] fields = _Type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            StringBuilder sb = new StringBuilder();
            for (int i = fields.Length - 1; i >= 0; i--)
            {
                sb.Append(GetFieldText(_obj, fields[i]));
            }
            var result = sb.ToString();
            if (string.IsNullOrEmpty(result))
            {
                _sb.Append(_tab + "param = nil");
            }
            else
            {
                _sb.AppendFormat(_tab + "param = {{{0}}},", result);
            }
        }

        private static string GetBTreeNodeTypeName(BTreeNode _root)
        {
            return _root.GetType().Name;
        }
        private static string GetBTreeNodePreconditionTypeName(BTreeNodePrecondition _condition)
        {
            return _condition.GetType().Name;
        }

        private static string GetFieldText(object _obj, FieldInfo _field)
        {
            if (_field == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            if (_field.FieldType == typeof(int))
            {
                sb.AppendFormat("{0} = {1}, ", _field.Name, (int)_field.GetValue(_obj));
            }
            else if (_field.FieldType == typeof(float))
            {
                sb.AppendFormat("{0} = {1}, ", _field.Name, (float)_field.GetValue(_obj));
            }
            else if (_field.FieldType == typeof(double))
            {
                sb.AppendFormat("{0} = {1}, ", _field.Name, (double)_field.GetValue(_obj));
            }
            else if (_field.FieldType == typeof(bool))
            {
                sb.AppendFormat("{0} = {1}, ", _field.Name, ((bool)_field.GetValue(_obj)).ToString().ToLower());
            }
            else if (_field.FieldType == typeof(string))
            {
                sb.AppendFormat("{0} = \"{1}\", ", _field.Name, (string)_field.GetValue(_obj));
            }
            else if (_field.FieldType.IsEnum)
            {
                sb.AppendFormat("{0} = {1}, ", _field.Name, (int)_field.GetValue(_obj));
            }
            else if (_field.FieldType == typeof(Vector2))
            {
                var value = (Vector2)_field.GetValue(_obj);
                sb.AppendFormat("{0} = {{x = {1}, y = {2}}}, ", _field.Name, value.x, value.y);
            }
            else if (_field.FieldType == typeof(Vector3))
            {
                var value = (Vector3)_field.GetValue(_obj);
                sb.AppendFormat("{0} = {{x = {1}, y = {2}, z = {3}}}, ", _field.Name, value.x, value.y, value.z);
            }
            else if (_field.FieldType == typeof(Vector4))
            {
                var value = (Vector4)_field.GetValue(_obj);
                sb.AppendFormat("{0} = {{x = {1}, y = {2}, z = {3}, w = {4}}}, ", _field.Name, value.x, value.y, value.z, value.w);
            }
            return sb.ToString();
        }
    }
}
