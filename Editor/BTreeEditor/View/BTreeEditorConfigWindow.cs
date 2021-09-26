using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace BTree.Editor
{
    public class BTreeEditorConfigWindow : EditorWindow
    {
        [MenuItem("BTree/BTree Config", false, 2)]
        public static void ShowWindow()
        {
            BTreeEditorConfigWindow bTreeEditorConfigWindow = GetWindow(typeof(BTreeEditorConfigWindow)) as BTreeEditorConfigWindow;
            bTreeEditorConfigWindow.titleContent = new GUIContent("行为树编辑器配置");
            DontDestroyOnLoad(bTreeEditorConfigWindow);
            bTreeEditorConfigWindow.Show();
        }

    }
}