using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BTree.Editor
{
    public class BTreeDebug
    {
        public static Dictionary<string, Type> sBTreeDebugConfig = new Dictionary<string, Type>{
            {"/Config/ai/", typeof(BTreeDebugNode_AI)},
            {"/Config/guide/", typeof(BTreeDebugNode_Guide)},
            {"/Config/weakGuide/", typeof(BTreeDebugNode_Guide)},
        };
        public static BTreeDebugNode EMPTY_DEBUG_NDOE = new BTreeDebugNode();
        public static Dictionary<string, List<BTreeDebugNode>> sBTreeNodeDic = new Dictionary<string, List<BTreeDebugNode>>();
        public static void SetBTreeNodeOnTick(string configPath, int[] indexs, object param)
        {
            configPath = configPath.StandardPath();
            Debug.Log("BTreeDebug:SetBTreeNodeOnTick:" + configPath);
            if (!string.IsNullOrEmpty(configPath))
            {
                var node = GetOrCreateNode(configPath, indexs);
                node.SetData(param);
            }
        }

        private static BTreeDebugNode GetOrCreateNode(string _configPath, int[] _indexs)
        {
            List<BTreeDebugNode> nodes;
            if (!sBTreeNodeDic.TryGetValue(_configPath, out nodes))
            {
                nodes = new List<BTreeDebugNode>();
                sBTreeNodeDic.Add(_configPath, nodes);
            }
            BTreeDebugNode node = FindeNode(_configPath, _indexs);
            if (node == null)
            {
                node = CreateNode(_configPath);
                node.SetIndexs(_indexs);
            }
            nodes.Add(node);
            return node;
        }
        private static BTreeDebugNode CreateNode(string _configPath)
        {
            foreach (var kv in sBTreeDebugConfig)
            {
                if (_configPath.Contains(kv.Key))
                {
                    ConstructorInfo constructor = kv.Value.GetConstructor(new Type[0]);
                    return constructor.Invoke(null) as BTreeDebugNode;
                }
            }
            return new BTreeDebugNode();
        }
        public static BTreeDebugNode FindeNode(string _configPath, object[] _indexs)
        {
            return FindeNode(_configPath, _indexs.Cast<int>().ToArray());
        }
        public static BTreeDebugNode FindeNode(string _configPath, int[] _indexs)
        {
            if (sBTreeNodeDic != null && !string.IsNullOrEmpty(_configPath))
            {
                List<BTreeDebugNode> nodes;
                if (sBTreeNodeDic.TryGetValue(_configPath, out nodes))
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (nodes[i].IsSelf(_indexs))
                        {
                            return nodes[i];
                        }
                    }
                }
            }
            return null;
        }
    }
}