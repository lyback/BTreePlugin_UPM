
namespace BTreeFrame
{
    /// <summary>
    /// class:      循环节点
    /// Evaluate:   预设的循环次数到了就返回False，否则，只调用第一个子节点的Evaluate方法，用它所返回的值作为自身的值返回
    /// Tick:       只调用第一个节点的Tick方法，若返回运行结束，则看是否需要重复运行，若循环次数没到，则自身返回运行中，若循环次数已到，则返回运行结束
    /// </summary>
    public class BTreeNodeSingleLoop : BTreeNodeCtrl
    {
        public int LoopCount;
        private int m_CurrentCount;
        private const int INFINITELOOP = -1;
        public BTreeNodeSingleLoop()
            : base()
        {
        }
        public BTreeNodeSingleLoop(BTreeNode _parentNode, BTreeNodePrecondition _precondition = null, int _loopCount = INFINITELOOP)
            : base(_parentNode, _precondition)
        {
            LoopCount = _loopCount;
            m_CurrentCount = 0;
        }

        protected override bool _DoEvaluate(BTreeTemplateData _input)
        {
            base._DoEvaluate(_input);
            bool checkLoopCount = LoopCount == INFINITELOOP || m_CurrentCount < LoopCount;
            if (!checkLoopCount)
            {
                return false;
            }
            if (_CheckIndex(0))
            {
                BTreeNode bn = m_ChildNodes[0];
                if (bn.Evaluate(_input))
                {
                    return true;
                }
            }
            return false;
        }
        protected override void _DoTransition(BTreeTemplateData _input)
        {
            base._DoTransition(_input);
            if (_CheckIndex(0))
            {
                BTreeNode bn = m_ChildNodes[0];
                bn.Transition(_input);
            }
            m_CurrentCount = 0;
        }
        protected override BTreeRunningStatus _DoTick(BTreeTemplateData _input, ref BTreeTemplateData _output)
        {
            BTreeRunningStatus runningStatus = base._DoTick(_input, ref _output);
            if (_CheckIndex(0))
            {
                BTreeNode bn = m_ChildNodes[0];
                runningStatus = bn.Tick(_input, ref _output);

                if (runningStatus == BTreeRunningStatus.Finish)
                {
                    m_CurrentCount++;
                    if (LoopCount != INFINITELOOP)
                    {
                        if (m_CurrentCount <= LoopCount)
                        {
                            runningStatus = BTreeRunningStatus.Executing;
                        }
                    }
                    else
                    {
                        runningStatus = BTreeRunningStatus.Executing;
                    }
                }
            }
            return runningStatus;
        }

        protected override void _DoReset()
        {
            base._DoReset();
            m_CurrentCount = 0;
        }
    }
}
