
//执行节点基类
namespace BTreeFrame
{
    public abstract class BTreeNodeAction : BTreeNode
    {
        private BTreeActionNodeStatus m_Status = BTreeActionNodeStatus.Ready;
        public BTreeActionNodeStatus Status
        {
            get
            {
                return m_Status;
            }
        }
        private bool m_NeedExit = false;

        public BTreeNodeAction()
            : base()
        {
            m_IsAcitonNode = true;
        }

        public BTreeNodeAction(BTreeNode _parentNode, BTreeNodePrecondition _precondition = null)
            : base(_parentNode, _precondition)
        {
            m_IsAcitonNode = true;
        }

        protected virtual void _DoEnter(BTreeTemplateData _input)
        {
            UnityEngine.Debug.Log("_DoEnter:" + Name);
        }
        protected virtual BTreeRunningStatus _DoExecute(BTreeTemplateData _input, ref BTreeTemplateData _output)
        {
            UnityEngine.Debug.Log("_DoExecute:" + Name);
            return BTreeRunningStatus.Finish;
        }
        protected virtual void _DoExit(BTreeTemplateData _input, BTreeRunningStatus _status)
        {
            UnityEngine.Debug.Log("_DoExit:" + Name);
        }

        protected override void _DoTransition(BTreeTemplateData _input)
        {
            base._DoTransition(_input);
            if (m_NeedExit)
            {
                _DoExit(_input, BTreeRunningStatus.Error);
            }
            SetActiveNode(null);
            m_Status = BTreeActionNodeStatus.Ready;
            m_NeedExit = false;
        }

        protected override BTreeRunningStatus _DoTick(BTreeTemplateData _input, ref BTreeTemplateData _output)
        {
            BTreeRunningStatus runningStatus = base._DoTick(_input, ref _output);
            if (m_Status == BTreeActionNodeStatus.Ready)
            {
                _DoEnter(_input);
                m_NeedExit = true;
                m_Status = BTreeActionNodeStatus.Running;
                SetActiveNode(this);
            }
            if (m_Status == BTreeActionNodeStatus.Running)
            {
                runningStatus = _DoExecute(_input, ref _output);
                SetActiveNode(this);
                if (runningStatus == BTreeRunningStatus.Finish || runningStatus == BTreeRunningStatus.Error)
                {
                    m_Status = BTreeActionNodeStatus.Finish;
                }
            }
            if (m_Status == BTreeActionNodeStatus.Finish)
            {
                if (m_NeedExit)
                {
                    _DoExit(_input, runningStatus);
                }
                m_Status = BTreeActionNodeStatus.Ready;
                m_NeedExit = false;
                SetActiveNode(null);
            }
            return runningStatus;
        }
    }
}
