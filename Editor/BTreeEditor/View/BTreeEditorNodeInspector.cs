
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BTreeFrame;

namespace BTree.Editor
{
    public class BTreeEditorNodeInspector
    {
        private UnityEngine.Object[] m_Preconditions = new UnityEngine.Object[20];
        private UnityEngine.Object m_ChangedPrecondition = null;
        private int m_Index = -1; //记录被修改的前提条件
        private int m_ClickIndex = -1; //记录点击的修改的前提条件
        private const int SPACEDETLE = 10;
        private static readonly List<Type> DrawFieldType = new List<Type> { typeof(int), typeof(float), typeof(double), typeof(bool), typeof(string), typeof(Vector2), typeof(Vector3), typeof(Vector4) };

        public void drawInspector(BTreeNodeDesigner _selectNode)
        {
            var _node = _selectNode.m_EditorNode.m_Node;
            var _type = _node.GetType();
            //绘制节点脚本
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Script:", new GUILayoutOption[] { GUILayout.Width(100) });
            EditorGUILayout.ObjectField("", FindMonoScriptAsset(_type.Name), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            //绘制节点名字
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", new GUILayoutOption[] { GUILayout.Width(100) });
            _node.Name = EditorGUILayout.TextField(_node.Name);
            GUILayout.EndHorizontal();

            //绘制节点条件以外的所有属性
            DrawAllFieldValue(_node);

            //绘制节点条件
            string commandName = Event.current.commandName;
            if (commandName == "ObjectSelectorClosed")
            {
                if (m_ClickIndex != -1)
                {
                    m_ChangedPrecondition = EditorGUIUtility.GetObjectPickerObject();
                    m_Index = m_ClickIndex;
                    m_ClickIndex = -1;
                    if (m_ChangedPrecondition != null)
                    {
                        Debug.Log("ObjectSelectorClosed：" + m_Index + "," + m_ChangedPrecondition.name);
                    }
                    else
                    {
                        Debug.Log("ObjectSelectorClosed：" + m_Index + "," + "null");
                    }
                }
            }
            GUILayout.Label("Precondition:");
            int index = -1;
            _node.m_NodePrecondition = DrawPrecondition(_node.GetNodePrecondition(), 0, ref index);
            GUILayout.FlexibleSpace();
        }

        BTreeNodePrecondition DrawPrecondition(BTreeNodePrecondition _condition, int _space, ref int _index)
        {
            _index = _index + 1;
            if (_index == m_Index)
            {
                if (m_ChangedPrecondition != null)
                {
                    if (_condition == null)
                    {
                        _condition = CreatePrecondition(m_ChangedPrecondition.name);
                    }
                    else
                    {
                        //不是旧的才替换
                        string oldName = _condition.GetType().Name;
                        if (oldName != m_ChangedPrecondition.name)
                        {
                            _condition = CreatePrecondition(m_ChangedPrecondition.name);
                        }
                    }
                }
                else
                {
                    _condition = null;
                }
                m_Index = -1;
                m_ChangedPrecondition = null;
            }
            if (_condition == null)
            {
                DrawObjField(null, typeof(MonoScript), _index, _space);
            }
            else
            {
                string name = _condition.GetType().Name;
                DrawObjField(FindMonoScriptAsset(name), typeof(MonoScript), _index, _space);
                DrawAllFieldValue(_condition, _space);
                _space = _space + SPACEDETLE;
                DrawPreconditionChilds(_condition, _space, ref _index);
            }
            return _condition;
        }
        void DrawPreconditionChilds(BTreeNodePrecondition _condition, int _space, ref int _index)
        {
            if (_condition is BTreeNodePreconditionAND)
            {
                var cond = _condition as BTreeNodePreconditionAND;
                int count = DrawPreconditionChildCountTxtField(cond.GetChildPreconditionCount(), _space);
                BTreeNodePrecondition[] childPreconditions = new BTreeNodePrecondition[count];
                BTreeNodePrecondition[] curChildPreconditions = cond.GetChildPrecondition();
                for (int i = 0; i < count; i++)
                {
                    if (i >= curChildPreconditions.Length)
                    {
                        childPreconditions[i] = DrawPrecondition(null, _space, ref _index);
                    }
                    else
                    {
                        childPreconditions[i] = DrawPrecondition(curChildPreconditions[i], _space, ref _index);
                    }
                }
                cond.SetChildPrecondition(childPreconditions);
            }
            else if (_condition is BTreeNodePreconditionOR)
            {
                var cond = _condition as BTreeNodePreconditionOR;
                int count = DrawPreconditionChildCountTxtField(cond.GetChildPreconditionCount(), _space);
                BTreeNodePrecondition[] childPreconditions = new BTreeNodePrecondition[count];
                BTreeNodePrecondition[] curChildPreconditions = cond.GetChildPrecondition();
                for (int i = 0; i < count; i++)
                {
                    if (i >= curChildPreconditions.Length)
                    {
                        childPreconditions[i] = DrawPrecondition(null, _space, ref _index);
                    }
                    else
                    {
                        childPreconditions[i] = DrawPrecondition(curChildPreconditions[i], _space, ref _index);
                    }
                }
                cond.SetChildPrecondition(childPreconditions);
            }
            else if (_condition is BTreeNodePreconditionNOT)
            {
                var cond = _condition as BTreeNodePreconditionNOT;
                cond.SetChildPrecondition(DrawPrecondition(cond.GetChildPrecondition(), _space, ref _index));
            }
        }
        int DrawPreconditionChildCountTxtField(int _count, int _space)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(_space);
            var val = EditorGUILayout.IntField(_count);
            GUILayout.EndHorizontal();
            return val;
        }
        void DrawObjField(UnityEngine.Object obj, Type type, int index, int space = 0)
        {
            var guiContent = EditorGUIUtility.ObjectContent(obj, type);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            var style = new GUIStyle("TextField");
            style.fixedHeight = 16;
            style.imagePosition = obj ? ImagePosition.ImageLeft : ImagePosition.TextOnly;
            if (GUILayout.Button(guiContent, style) && obj)
                EditorGUIUtility.PingObject(obj);

            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
            {
                int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
                m_ClickIndex = index;
                // Debug.Log("DrawObjField Add Click:" + m_ClickIndex);
                EditorGUIUtility.ShowObjectPicker<MonoScript>(obj, false, "l:Btree_Precondition", controlID);
            }
            EditorGUILayout.EndHorizontal();
        }
        void DrawAllFieldValue(object _obj, int _space = 0)
        {
            Type _Type = _obj.GetType();
            FieldInfo[] fields = _Type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType.IsEnum || DrawFieldType.Contains(fields[i].FieldType))
                {
                    DrawValue(_obj, fields[i], _space);
                }
            }
        }
        void DrawValue(object _obj, FieldInfo _field, int _space = 0)
        {
            if (_field == null)
            {
                return;
            }
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(_space);
                EditorGUILayout.LabelField(BTreeEditorUtility.SplitCamelCase(_field.Name) + ":", new GUILayoutOption[] { GUILayout.Width(100) });
                if (_field.FieldType == typeof(int))
                {
                    var _val = EditorGUILayout.IntField((int)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType == typeof(float))
                {
                    var _val = EditorGUILayout.FloatField((float)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType == typeof(double))
                {
                    var _val = EditorGUILayout.DoubleField((double)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType == typeof(string))
                {
                    var _val = EditorGUILayout.TextField((string)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType == typeof(bool))
                {
                    var _val = EditorGUILayout.Toggle((bool)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType.IsEnum)
                {
                    var _val = EditorGUILayout.EnumPopup((Enum)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType == typeof(Vector2))
                {
                    var _val = EditorGUILayout.Vector2Field("", (Vector2)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType == typeof(Vector3))
                {
                    var _val = EditorGUILayout.Vector3Field("", (Vector3)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                else if (_field.FieldType == typeof(Vector4))
                {
                    var _val = EditorGUILayout.Vector4Field("", (Vector4)(_field.GetValue(_obj)));
                    _field.SetValue(_obj, _val);
                }
                GUILayout.EndHorizontal();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(e.Message);
            }
        }
        private void openInFileEditor(object _node)
        {
            UnityEngine.Debug.Log(_node.GetType().Name);

        }
        private BTreeNodePrecondition CreatePrecondition(string name)
        {
            if (name != null)
            {
                Type type = GetPreconditionType(name);
                if (type == null)
                {
                    Debug.LogError("Cant Find Precondition Type of " + name);
                    return null;
                }
                return (BTreeNodePrecondition)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            return null;
        }
        private Type GetPreconditionType(string name)
        {
            Assembly assembly = Assembly.Load("Assembly-CSharp-Editor");
            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    if (!types[j].IsAbstract)
                    {
                        if (types[j].IsSubclassOf(typeof(BTreeNodePrecondition)) && types[j].Name == name)
                        {
                            return types[j];
                        }
                    }
                }
            }
            return null;
        }
        private Dictionary<string, MonoScript> MonoScriptCache = new Dictionary<string, MonoScript>();
        private MonoScript FindMonoScriptAsset(string name)
        {
            if (MonoScriptCache.ContainsKey(name))
            {
                return MonoScriptCache[name];
            }
            string[] scripts = AssetDatabase.FindAssets("t:Script " + name);
            if (scripts.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(scripts[0]);
                MonoScript monoScript = (MonoScript)AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));
                if (MonoScriptCache.ContainsKey(name))
                {
                    MonoScriptCache[name] = monoScript;
                }
                else
                {
                    MonoScriptCache.Add(name, monoScript);
                }
                return monoScript;
            }
            return null;
        }
    }
}
