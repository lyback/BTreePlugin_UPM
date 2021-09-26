
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace BTree.Editor
{
    public class BTreeEditorWindow : EditorWindow
    {
        public static BTreeEditorWindow Instance;
        public static List<BTreeNodeDesigner> m_ClipboardNodes = new List<BTreeNodeDesigner>();

        [MenuItem("BTree/BTree Window %#_t", false, 1)]
        public static void ShowWindow()
        {
            BTreeEditorWindow bTreeEditorWindow = CreateInstance<BTreeEditorWindow>();
            // BTreeEditorWindow bTreeEditorWindow = GetWindow(typeof(BTreeEditorWindow)) as BTreeEditorWindow;
            bTreeEditorWindow.titleContent = new GUIContent("行为树编辑器");
            bTreeEditorWindow.mIsFirst = true;
            bTreeEditorWindow.wantsMouseMove = true;
            bTreeEditorWindow.minSize = new Vector2(600f, 500f);
            DontDestroyOnLoad(bTreeEditorWindow);
            bTreeEditorWindow.Show();
        }
        private BTreeGraphDesigner _mGraphDesigner;
        private BTreeGraphDesigner mGraphDesigner
        {
            get
            {
                if (_mGraphDesigner == null)
                {
                    _mGraphDesigner = new BTreeGraphDesigner();
                }
                return _mGraphDesigner;
            }
            set
            {
                _mGraphDesigner = value;
            }
        }

        private Rect mGraphRect;
        private Rect mFileToolBarRect;
        private Rect mPropertyToolbarRect;
        private Rect mPropertyBoxRect;
        private Rect mPreferencesPaneRect;

        private Vector2 mCurrentMousePosition = Vector2.zero;

        private Vector2 mGraphScrollSize = new Vector2(20000f, 20000f);
        private Vector2 mGraphScrollPosition = new Vector2(-1f, -1f);
        private Vector2 mGraphOffset = Vector2.zero;
        private float mGraphZoom = 1f;

        //是否显示设置界面
        private bool mShowPrefPanel;
        //是否点击节点中
        private bool mNodeClicked;
        //是否在拖动中
        private bool mIsDragging;
        //是否在连线状态
        private bool mIsConnectingLine;
        //当前鼠标位置的节点
        private BTreeNodeDesigner mCurMousePosNode;

        //右键菜单
        private bool mIsRightClickMenu;//是否显示右键菜单
        private BTreeEditorRightClickBlockMenu mRightClickBlockMenu = null;
        private bool mIsRightClickBlockMenuShow;
        private BTreeEditorRightClickNodeMenu mRightClickNodeMenu = null;
        private bool mIsRightClickNodeMenuShow;

        //属性栏
        private BTreeEditorNodeInspector mNodeInspector = new BTreeEditorNodeInspector();

        //网格材质
        private Material m_GridMaterial;

        private bool mIsFirst;

        public void OnGUI()
        {
            if (mIsFirst)
            {
                mIsFirst = false;
            }
            mCurrentMousePosition = Event.current.mousePosition;
            setupSizes();
            ShowMenu();
            handleEvents();
            if (Draw())
            {
                Repaint();
            }
        }
        private void ShowMenu()
        {
            if (mIsRightClickBlockMenuShow)
            {
                mIsRightClickBlockMenuShow = false;
                if (mRightClickBlockMenu == null)
                {
                    mRightClickBlockMenu = new BTreeEditorRightClickBlockMenu(this);
                }
                mIsRightClickMenu = true;
                mRightClickBlockMenu.ShowAsContext();
            }
            if (mIsRightClickNodeMenuShow)
            {
                mIsRightClickNodeMenuShow = false;
                if (mRightClickNodeMenu == null)
                {
                    mRightClickNodeMenu = new BTreeEditorRightClickNodeMenu(this);
                }
                mIsRightClickMenu = true;
                mRightClickNodeMenu.ShowAsContext(mGraphDesigner.m_SelectedNodes);
            }
        }
        public bool Draw()
        {
            bool result = false;
            Color color = GUI.color;
            Color backgroundColor = GUI.backgroundColor;
            GUI.color = (Color.white);
            GUI.backgroundColor = (Color.white);
            drawFileToolbar();
            drawPropertiesBox();
            if (drawGraphArea())
            {
                result = true;
            }
            GUI.color = color;
            GUI.backgroundColor = backgroundColor;
            return result;
        }
        private void setupSizes()
        {

            mFileToolBarRect = new Rect(BTreeEditorUtility.PropertyBoxWidth, 0f, (Screen.width - BTreeEditorUtility.PropertyBoxWidth), BTreeEditorUtility.ToolBarHeight);
            mPropertyToolbarRect = new Rect(0f, 0f, BTreeEditorUtility.PropertyBoxWidth, BTreeEditorUtility.ToolBarHeight);
            mPropertyBoxRect = new Rect(0f, mPropertyToolbarRect.height, BTreeEditorUtility.PropertyBoxWidth, Screen.height - mPropertyToolbarRect.height - BTreeEditorUtility.EditorWindowTabHeight);
            mGraphRect = new Rect(BTreeEditorUtility.PropertyBoxWidth, BTreeEditorUtility.ToolBarHeight, (Screen.width - BTreeEditorUtility.PropertyBoxWidth - BTreeEditorUtility.ScrollBarSize), (Screen.height - BTreeEditorUtility.ToolBarHeight - BTreeEditorUtility.EditorWindowTabHeight - BTreeEditorUtility.ScrollBarSize));
            mPreferencesPaneRect = new Rect(BTreeEditorUtility.PropertyBoxWidth + mGraphRect.width - BTreeEditorUtility.PreferencesPaneWidth, (BTreeEditorUtility.ToolBarHeight + (EditorGUIUtility.isProSkin ? 1 : 2)), BTreeEditorUtility.PreferencesPaneWidth, BTreeEditorUtility.PreferencesPaneHeight);

            if (mGraphScrollPosition == new Vector2(-1f, -1f))
            {
                mGraphScrollPosition = new Vector2(1f, 1f);
                // mGraphScrollPosition = (mGraphScrollSize - new Vector2(mGraphRect.width, mGraphRect.height)) / 2f - 2f * new Vector2(BTreeEditorUtility.ScrollBarSize, BTreeEditorUtility.ScrollBarSize);
            }
        }
        //绘制图形区域
        private bool drawGraphArea()
        {
            Vector2 vector = GUI.BeginScrollView(new Rect(mGraphRect.x, mGraphRect.y, mGraphRect.width + BTreeEditorUtility.ScrollBarSize, mGraphRect.height + BTreeEditorUtility.ScrollBarSize), mGraphScrollPosition, new Rect(0f, 0f, mGraphScrollSize.x, mGraphScrollSize.y), true, true);
            if (vector != mGraphScrollPosition && Event.current.type != EventType.DragUpdated && Event.current.type != EventType.Ignore)
            {
                mGraphOffset -= (vector - mGraphScrollPosition) / mGraphZoom;
                mGraphScrollPosition = vector;
                mGraphDesigner.GraphDirty();
            }
            GUI.EndScrollView();
            GUI.Box(mGraphRect, "", BTreeEditorUtility.GraphBackgroundGUIStyle);

            BTreeEditorZoomArea.Begin(mGraphRect, mGraphZoom);
            Vector2 mousePosition;
            if (!getMousePositionInGraph(out mousePosition))
            {
                mousePosition = new Vector2(-1f, -1f);
            }
            bool result = false;
            if (mGraphDesigner.drawNodes(mousePosition, mGraphOffset, mGraphZoom))
            {
                result = true;
            }
            if (mIsConnectingLine)
            {
                var _curNode = mGraphDesigner.nodeAt(mousePosition, mGraphOffset);
                if (_curNode != null)
                {
                    Debug.Log(_curNode.m_EditorNode.m_Pos + mGraphOffset);
                }
                Vector2 des = _curNode == null ? mousePosition : _curNode.m_EditorNode.m_Pos + mGraphOffset;
                mGraphDesigner.drawTempConnection(des, mGraphOffset, mGraphZoom);
            }
            BTreeEditorZoomArea.End();
            return result;
        }
        //绘制工具栏
        private void drawFileToolbar()
        {
            GUILayout.BeginArea(mFileToolBarRect, EditorStyles.toolbar);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Clean", EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.Width(80f)
            }))
            {
                ReSet();
                _mGraphDesigner = new BTreeGraphDesigner();
            }
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.Width(80f)
            }))
            {
                loadBTree();
            }
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.Width(80f)
            }))
            {
                saveBTree();
            }
            if (GUILayout.Button("Save As", EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.Width(80f)
            }))
            {
                saveBTreeAtPath();
            }
            if (GUILayout.Button("ExportToLua", EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.Width(100f)
            }))
            {
                exportBtreeToLua();
            }
            if (GUILayout.Button("ExportAllGuideLua", EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.Width(100f)
            }))
            {
                exportAllGuideBtreeToLua();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Preferences", mShowPrefPanel ? BTreeEditorUtility.ToolbarButtonSelectionGUIStyle : EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.Width(80f)
            }))
            {
                mShowPrefPanel = !mShowPrefPanel;
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        //绘制属性栏
        private void drawPropertiesBox()
        {
            GUILayout.BeginArea(mPropertyToolbarRect, EditorStyles.toolbar);
            GUILayout.EndArea();
            GUILayout.BeginArea(mPropertyBoxRect, BTreeEditorUtility.PropertyBoxGUIStyle);
            if (mGraphDesigner.m_SelectedNodes.Count == 1)
            {
                mNodeInspector.drawInspector(mGraphDesigner.m_SelectedNodes[0]);
            }
            GUILayout.EndArea();
        }
        #region 操作消息处理相关
        //获取鼠标位置是否在绘图区域内
        private bool getMousePositionInGraph(out Vector2 mousePosition)
        {
            mousePosition = mCurrentMousePosition;
            if (!mGraphRect.Contains(mousePosition))
            {
                return false;
            }
            if (mShowPrefPanel && mPreferencesPaneRect.Contains(mousePosition))
            {
                return false;
            }
            mousePosition -= new Vector2(mGraphRect.xMin, mGraphRect.yMin);
            mousePosition /= mGraphZoom;
            return true;
        }
        //处理操作消息
        private void handleEvents()
        {
            if (EditorApplication.isCompiling) return;

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        // Debug.Log("leftMouseDown");
                        if (leftMouseDown(e.clickCount))
                        {
                            e.Use();
                            return;
                        }
                    }
                    else if (e.button == 1)
                    {
                        // Debug.Log("rightMouseDown");
                        if (rightMouseDown())
                        {
                            e.Use();
                            return;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        // Debug.Log("leftMouseRelease");
                        if (leftMouseRelease())
                        {
                            e.Use();
                            return;
                        }
                    }
                    else if (e.button == 1)
                    {
                        // Debug.Log("rightMouseRelease");
                        if (rightMouseRelease())
                        {
                            e.Use();
                            return;
                        }
                    }
                    break;
                case EventType.ContextClick:
                    // Debug.Log("EventType.ContextClick");
                    break;
                case EventType.MouseMove:
                    if (mouseMove())
                    {
                        e.Use();
                        return;
                    }
                    break;
                case EventType.MouseDrag:
                    if (mIsRightClickMenu)
                    {
                        mIsRightClickMenu = false;
                        return;
                    }
                    if (e.button == 0)
                    {
                        if (leftMouseDragged())
                        {
                            e.Use();
                            return;
                        }
                        if (e.modifiers == EventModifiers.Alt && mousePan())
                        {
                            e.Use();
                            return;
                        }
                    }
                    else if (e.button == 2 && mousePan())
                    {
                        e.Use();
                        return;
                    }
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.KeyUp:
                    break;
                case EventType.ScrollWheel:
                    if (mIsRightClickMenu)
                    {
                        return;
                    }
                    if (mouseZoom())
                    {
                        e.Use();
                        return;
                    }
                    break;
                default:
                    break;
            }
        }
        //鼠标移动
        private bool mouseMove()
        {
            return true;
        }
        //鼠标左键down
        private bool leftMouseDown(int clickCount)
        {
            Vector2 point;
            if (!getMousePositionInGraph(out point))
            {
                mIsConnectingLine = false;
                return false;
            }
            var nodeDesigner = mGraphDesigner.nodeAt(point, mGraphOffset);
            if (mIsConnectingLine && nodeDesigner != null)
            {
                mGraphDesigner.addSelectNodeLine(nodeDesigner);
            }
            mGraphDesigner.clearNodeSelection();
            if (nodeDesigner != null)
            {
                mGraphDesigner.select(nodeDesigner);
                mNodeClicked = true;
            }
            mIsConnectingLine = false;
            return true;
        }
        private bool leftMouseDragged()
        {
            Vector2 point;
            if (!getMousePositionInGraph(out point))
            {
                return false;
            }
            if (mNodeClicked)
            {
                bool flag = mGraphDesigner.dragSelectedNodes(Event.current.delta / mGraphZoom, Event.current.modifiers != EventModifiers.Alt, mIsDragging);
                if (flag)
                {
                    mIsDragging = true;
                }
            }
            return true;
        }
        //鼠标左键Release
        private bool leftMouseRelease()
        {
            Vector2 point;
            if (!getMousePositionInGraph(out point))
            {
                return false;
            }
            mNodeClicked = false;
            return true;
        }
        //鼠标右键down
        private bool rightMouseDown()
        {
            Vector2 point;
            mIsConnectingLine = false;
            if (!getMousePositionInGraph(out point))
            {
                return false;
            }
            // mGraphDesigner.clearNodeSelection();
            // var nodeDesigner = mGraphDesigner.nodeAt(point, mGraphOffset);
            // if (nodeDesigner != null)
            // {
            //     mGraphDesigner.select(nodeDesigner);
            //     mNodeClicked = true;
            // }
            return true;
        }
        //鼠标右键Release
        private bool rightMouseRelease()
        {
            Vector2 point;
            if (!getMousePositionInGraph(out point))
            {
                return false;
            }

            mGraphDesigner.clearNodeSelection();
            var nodeDesigner = mGraphDesigner.nodeAt(point, mGraphOffset);
            if (nodeDesigner != null)
            {
                mGraphDesigner.select(nodeDesigner);
                mNodeClicked = true;
            }

            if (mGraphDesigner.m_SelectedNodes != null && mGraphDesigner.m_SelectedNodes.Count != 0)
            {
                mIsRightClickNodeMenuShow = true;
                return true;
            }
            else
            {
                mIsRightClickBlockMenuShow = true;
                return true;
            }
        }
        //缩放
        private bool mouseZoom()
        {
            Vector2 vector;
            Vector2 vector2;
            if (!getMousePositionInGraph(out vector))
            {
                return false;
            }
            float num = -Event.current.delta.y / 150f;
            this.mGraphZoom += num;
            this.mGraphZoom = Mathf.Clamp(this.mGraphZoom, 0.4f, 1f);
            getMousePositionInGraph(out vector2);
            this.mGraphOffset += vector2 - vector;
            this.mGraphScrollPosition += vector2 - vector;
            this.mGraphDesigner.GraphDirty();
            return true;
        }
        //平移
        private bool mousePan()
        {
            Vector2 vector;
            if (!getMousePositionInGraph(out vector))
            {
                return false;
            }
            Vector2 amount = Event.current.delta;
            if (Event.current.type == EventType.ScrollWheel)
            {
                amount = (Vector2)(amount * -1.5f);
                if (Event.current.modifiers == EventModifiers.Control)
                {
                    amount.x = amount.y;
                    amount.y = 0f;
                }
            }
            this.ScrollGraph(amount);
            return true;
        }
        private void ScrollGraph(Vector2 amount)
        {
            this.mGraphOffset += (Vector2)(amount / this.mGraphZoom);
            this.mGraphScrollPosition -= amount;
            this.mGraphDesigner.GraphDirty();
            base.Repaint();
        }
        //添加节点
        private void addNode(Type type, bool useMousePosition)
        {
            Vector2 vector = new Vector2(mGraphRect.width / (2f * mGraphZoom), 150f);
            if (useMousePosition)
            {
                getMousePositionInGraph(out vector);
            }
            vector -= mGraphOffset;
            if (mGraphDesigner.addNode(type, vector) != null)
            {
                UnityEngine.Debug.Log("addNode");
            }
        }
        //禁用节点
        private void disableSelectNode()
        {
            mGraphDesigner.disableNodeSelection();
        }
        //启用节点
        private void enableSelectNode()
        {
            mGraphDesigner.enableNodeSelection();
        }
        //删除节点
        private void delectSelectNode()
        {
            mGraphDesigner.delectSelectNode();
        }
        //设置入口节点
        private void setSelectNodeAsEntry()
        {
            mGraphDesigner.setSelectNodeAsEntry();
        }
        //前移节点
        private void moveUpSelectNodeIndex()
        {
            mGraphDesigner.moveUpSelectNodeIndex();
        }
        //后移节点
        private void moveDownSelectNodeIndex()
        {
            mGraphDesigner.moveDownSelectNodeIndex();
        }
        //置顶节点
        private void moveSelectNodeIndexToTop()
        {
            mGraphDesigner.moveSelectNodeIndexToTop();
        }
        //置尾节点
        private void moveSelectNodeIndexToEnd()
        {
            mGraphDesigner.moveSelectNodeIndexToEnd();
        }
        //断开节点
        private void disconnectSelectNode()
        {
            mGraphDesigner.disconnectSelectNode();
        }
        //复制节点
        private void copySelectNode()
        {
            m_ClipboardNodes = mGraphDesigner.copySelectNode();
        }
        //粘贴节点
        private void pasteSelectNode()
        {
            Vector2 point;
            getMousePositionInGraph(out point);
            if (m_ClipboardNodes != null && m_ClipboardNodes.Count > 0)
            {
                mGraphDesigner.pasteSelectNode(m_ClipboardNodes, point);
            }
            m_ClipboardNodes.Clear();
        }
        #endregion

        #region 配置文件相关
        static string sOpenFilePath = "";
        public void loadBTree()
        {
            string text = EditorUtility.OpenFilePanel("Load Behavior Tree", BTreeEditorHelper.EditorConfigPath, "json");
            if (!string.IsNullOrEmpty(text))
            {
                UnityEngine.Debug.Log("loadBTree:" + text);
                sOpenFilePath = text;
                string json = BTreeEditorHelper.ReadFileAtPath(text);
                BTreeEditorConfig config = BTreeEditorHelper.FromJson<BTreeEditorConfig>(json);
                mGraphDesigner = BTreeEditorNodeFactory.BtreeEditorConfig_TO_BTreeGraphDesigner(config);
            }
        }
        public void saveBTree()
        {
            if (mGraphDesigner == null || mGraphDesigner.m_RootNode == null)
            {
                EditorUtility.DisplayDialog("Save Error", "未创建根节点", "ok");
                return;
            }
            if (!string.IsNullOrEmpty(sOpenFilePath))
            {
                string _config = BTreeEditorNodeFactory.BTreeGraphDesigner_TO_BtreeEditorConfig(mGraphDesigner);
                BTreeEditorHelper.WirteFileAtPath(_config, sOpenFilePath);
                EditorUtility.DisplayDialog("Save", "保存行为树编辑器成功:" + sOpenFilePath, "ok");
            }
            else
            {
                saveBTreeAtPath();
            }
        }
        public void saveBTreeAtPath()
        {
            if (mGraphDesigner == null || mGraphDesigner.m_RootNode == null)
            {
                EditorUtility.DisplayDialog("Save Error", "未创建根节点", "ok");
                return;
            }
            string suffix = "json";
            string text = EditorUtility.SaveFilePanel("Save Behavior Tree", BTreeEditorHelper.EditorConfigPath, mGraphDesigner.m_RootNode.m_NodeName, suffix);
            if (!string.IsNullOrEmpty(text))
            {
                string _config = BTreeEditorNodeFactory.BTreeGraphDesigner_TO_BtreeEditorConfig(mGraphDesigner);
                BTreeEditorHelper.WirteFileAtPath(_config, text);
                EditorUtility.DisplayDialog("Save", "保存行为树编辑器成功:" + text, "ok");
                sOpenFilePath = text;
            }
        }
        public void exportBtreeToLua()
        {
            if (mGraphDesigner == null || mGraphDesigner.m_RootNode == null)
            {
                EditorUtility.DisplayDialog("Export Error", "未创建根节点", "ok");
                return;
            }
            string suffix = "lua";
            string text = EditorUtility.SaveFilePanel("Export Behavior Tree Lua", BTreeEditorHelper.LuaConfigPath, mGraphDesigner.m_RootNode.m_NodeName, suffix);
            if (!string.IsNullOrEmpty(text))
            {
                UnityEngine.Debug.Log("exportBtreeToLua");
                string lua = BTreeEditorNodeFactory.BTreeNode_TO_Lua(mGraphDesigner.m_RootNode);
                BTreeEditorHelper.WirteFileAtPath(lua, text);
                EditorUtility.DisplayDialog("Export", "导出行为树Lua:" + text, "ok");
            }
        }
        public void exportAllGuideBtreeToLua()
        {
            List<string> ConfigFiles = FileHelper.GetAllChildFiles(BTreeEditorHelper.EditorConfigPath + "guide", ".json");
            ConfigFiles.AddRange(FileHelper.GetAllChildFiles(BTreeEditorHelper.EditorConfigPath + "weakGuide", ".json"));
            for (int i = 0; i < ConfigFiles.Count; i++)
            {
                string fileName = ConfigFiles[i].GetFileName().RemoveSuffix();
                string json = BTreeEditorHelper.ReadFileAtPath(ConfigFiles[i]);
                BTreeEditorConfig config = BTreeEditorHelper.FromJson<BTreeEditorConfig>(json);
                var _GraphDesigner = BTreeEditorNodeFactory.BtreeEditorConfig_TO_BTreeGraphDesigner(config);
                string lua = BTreeEditorNodeFactory.BTreeNode_TO_Lua(_GraphDesigner.m_RootNode);
                BTreeEditorHelper.WirteFileAtPath(lua, BTreeEditorHelper.LuaConfigPath + "guide/" + fileName + ".lua");
            }
            EditorUtility.DisplayDialog("Export", "导出所有引导行为树Lua成功", "ok");
        }
        public void ReSet()
        {
            sOpenFilePath = "";
        }
        #endregion
        #region 右键菜单点击回调
        public void addNodeCallback(object node)
        {
            addNode((Type)node, true);
        }
        public void disableNodeCallback()
        {
            disableSelectNode();
        }
        public void enableNodeCallback()
        {
            enableSelectNode();
        }
        public void delectNodeCallback()
        {
            delectSelectNode();
        }
        public void connectLineCallback()
        {
            mIsConnectingLine = true;
        }
        public void disconnectCallback()
        {
            disconnectSelectNode();
        }
        public void setEntryNodeCallback()
        {
            setSelectNodeAsEntry();
        }
        public void moveUpIndexCallback()
        {
            moveUpSelectNodeIndex();
        }
        public void moveDownIndexCallback()
        {
            moveDownSelectNodeIndex();
        }
        public void moveIndexToTopCallback()
        {
            moveSelectNodeIndexToTop();
        }
        public void moveIndexToEndCallback()
        {
            moveSelectNodeIndexToEnd();
        }
        public void copyNodeCallback()
        {
            copySelectNode();
        }
        public void pasteNodeCallback()
        {
            pasteSelectNode();
        }
        #endregion
    }
}
