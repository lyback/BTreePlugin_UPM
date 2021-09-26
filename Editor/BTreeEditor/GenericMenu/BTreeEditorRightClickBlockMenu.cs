
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BTreeFrame;

namespace BTree.Editor
{
    public class BTreeEditorRightClickBlockMenu : BTreeEditorGenericMenuBase
    {
        public BTreeEditorRightClickBlockMenu(BTreeEditorWindow _window)
            : base(_window)
        {
            Init();
        }
        public void Init()
        {
            m_Menu = new GenericMenu();
            
            List<Type> actionList = new List<Type>();
            List<Type> selectorList = new List<Type>();
            List<Assembly> assemblys = new List<Assembly>();

            string[] path = FileHelper.GetFiles(Application.dataPath + "/../", "*.csproj", System.IO.SearchOption.TopDirectoryOnly);
            for (int i = 0; i < path.Length; i++)
            {
                string name = path[i].GetFileName().RemoveSuffix();
                Debug.Log(name);
                try
                {
                    assemblys.Add(Assembly.Load(name));
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            for (int i = 0; i < assemblys.Count; i++)
            {
                Assembly assembly = assemblys[i];
                if (assembly != null)
                {
                    Type[] types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (!types[j].IsAbstract)
                        {
                            if (types[j].IsSubclassOf(typeof(BTreeNodeAction)))
                            {
                                actionList.Add(types[j]);
                            }
                            else if (types[j].IsSubclassOf(typeof(BTreeNodeCtrl)))
                            {
                                selectorList.Add(types[j]);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < actionList.Count; i++)
            {
                string sub = actionList[i].Name.Split('_')[0];
                AddItem(new GUIContent(string.Format("Add {0} Action/", sub) + actionList[i].Name), false, new GenericMenu.MenuFunction2(AddNodeCallback), actionList[i]);
            }
            for (int i = 0; i < selectorList.Count; i++)
            {
                AddItem(new GUIContent("Add Task Selector/" + selectorList[i].Name), false, new GenericMenu.MenuFunction2(AddNodeCallback), selectorList[i]);
            }
            AddSeparator("");
            if (BTreeEditorWindow.m_ClipboardNodes != null && BTreeEditorWindow.m_ClipboardNodes.Count > 0)
            {
                AddItem(new GUIContent("Paste Node"), false, new GenericMenu.MenuFunction(PasteNodeCallback));
            }
            else
            {
                AddDisabledItem(new GUIContent("Paste Node"));
            }
        }

        private void AddNodeCallback(object obj)
        {
            m_Window.addNodeCallback(obj);
        }

        private void PasteNodeCallback()
        {
            m_Window.pasteNodeCallback();
        }

        public override void ShowAsContext()
        {
            Init();
            base.ShowAsContext();
        }
    }
}