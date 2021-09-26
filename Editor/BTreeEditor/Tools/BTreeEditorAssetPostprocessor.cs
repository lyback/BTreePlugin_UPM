using UnityEngine;
using UnityEditor;

namespace BTree.Editor
{
    public class BTreeEditorAssetPostprocessor : AssetPostprocessor
    {

        //所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAsset)
            {
                if (str.Contains(BTreeEditorHelper.PreconditionPath) || str.Contains(BTreeEditorHelper.PreconditionBasePath))
                {
                    Debug.Log("OnPostprocessAllAssets:设置行为树前提条件Labels———>" + str);
                    AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<MonoScript>(str), BTreeEditorHelper.PreconditionLabel);
                }
            }
            foreach (string str in movedAssets)
            {
                if (str.Contains(BTreeEditorHelper.PreconditionPath) || str.Contains(BTreeEditorHelper.PreconditionBasePath))
                {
                    Debug.Log("OnPostprocessAllAssets:设置行为树前提条件Labels———>" + str);
                    AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<MonoScript>(str), BTreeEditorHelper.PreconditionLabel);
                }
            }
        }
    }
}