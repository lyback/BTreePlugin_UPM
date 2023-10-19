
using System.Collections.Generic;
using BTreeFrame;
using UnityEngine;

namespace BTree.Editor
{
    class BTreeEditorNodeFactory
    {
        #region 从行为树编辑器类生成配置
        public static string BTreeGraphDesigner_TO_BtreeEditorConfig(BTreeGraphDesigner _graphDesigner)
        {
            BTreeEditorConfig _config = new BTreeEditorConfig();
            _config.m_EnterTreeNode = BTreeNodeDesigner_TO_BTreeEditorNodeConfig(_graphDesigner.m_RootNode);
            for (int i = 0; i < _graphDesigner.m_DetachedNodes.Count; i++)
            {
                BTreeEditorNodeConfig detachNode = BTreeNodeDesigner_TO_BTreeEditorNodeConfig(_graphDesigner.m_DetachedNodes[i]);
                _config.m_DetachedTreeNode.Add(detachNode);
            }
            return BTreeEditorHelper.ToJson(_config);
        }
        private static BTreeEditorNodeConfig BTreeNodeDesigner_TO_BTreeEditorNodeConfig(BTreeNodeDesigner nodeDesigner)
        {
            BTreeEditorNodeConfig _config = new BTreeEditorNodeConfig();
            _config.m_RootNodeJson = BTreeEditorHelper.ToJson(nodeDesigner.m_EditorNode);
            for (int i = 0; i < nodeDesigner.m_ChildNodeList.Count; i++)
            {
                _config.m_ChildNodes.Add(BTreeNodeDesigner_TO_BTreeEditorNodeConfig(nodeDesigner.m_ChildNodeList[i]));
            }
            return _config;
        }
        #endregion
        #region 从配置生成行为树编辑器相关方法
        public static BTreeGraphDesigner BtreeEditorConfig_TO_BTreeGraphDesigner(BTreeEditorConfig _config, string _configPath)
        {
            _configPath = _configPath.ReplaceEmpty(Application.dataPath.ReplaceEmpty("Assets"));
            BTreeGraphDesigner _graphDesigner = new BTreeGraphDesigner(_configPath);
            _graphDesigner.m_RootNode = BTreeEditorNodeConfig_TO_BTreeNodeDesigner(_config.m_EnterTreeNode, _configPath, null);
            _graphDesigner.m_RootNode.SetEntryDisplay(true);
            _graphDesigner.m_DetachedNodes = new List<BTreeNodeDesigner>();
            for (int i = 0; i < _config.m_DetachedTreeNode.Count; i++)
            {
                _graphDesigner.m_DetachedNodes.Add(BTreeEditorNodeConfig_TO_BTreeNodeDesigner(_config.m_DetachedTreeNode[i], _configPath, null));
            }
            return _graphDesigner;
        }
        private static BTreeNodeDesigner BTreeEditorNodeConfig_TO_BTreeNodeDesigner(BTreeEditorNodeConfig _config, string _configKey, BTreeNodeDesigner _parent = null)
        {
            BTreeEditorNode editorNode = BTreeEditorHelper.FromJson<BTreeEditorNode>(_config.m_RootNodeJson);
            BTreeNodeDesigner nodeDesigner = new BTreeNodeDesigner(editorNode, _configKey);
            if (_parent != null)
            {
                nodeDesigner.m_ParentNode = _parent;
                nodeDesigner.m_ParentNodeConnection = new BTreeNodeConnection(nodeDesigner, _parent, NodeConnectionType.Incoming);
            }
            List<BTreeNodeDesigner> childNodeDesigner = new List<BTreeNodeDesigner>();
            for (int i = 0; i < _config.m_ChildNodes.Count; i++)
            {
                childNodeDesigner.Add(BTreeEditorNodeConfig_TO_BTreeNodeDesigner(_config.m_ChildNodes[i], _configKey, nodeDesigner));
            }
            nodeDesigner.m_ChildNodeList = childNodeDesigner;
            //BtreeNode重新绑定引用
            if (_parent != null)
            {
                nodeDesigner.m_EditorNode.m_Node.m_ParentNode = _parent.m_EditorNode.m_Node;
            }
            nodeDesigner.m_EditorNode.m_Node.ClearChildNode();
            for (int i = 0; i < childNodeDesigner.Count; i++)
            {
                nodeDesigner.m_EditorNode.AddChildNode(childNodeDesigner[i].m_EditorNode);
            }
            //绘制连线
            for (int i = 0; i < childNodeDesigner.Count; i++)
            {
                nodeDesigner.m_ChildNodeConnectionList.Add(new BTreeNodeConnection(childNodeDesigner[i], nodeDesigner, NodeConnectionType.Outgoing));
            }
            return nodeDesigner;
        }
        #endregion
        #region 从行为树编辑器类生成Lua运行时配置
        public static string BTreeNode_TO_Lua(BTreeNodeDesigner _dRoot, string _configPath)
        {
            return BTreeEditorGenLua.GenLuaFromBTreeNode(_dRoot, _configPath);
        }
        #endregion
    }
}
