
namespace BTreeFrame
{
    /// <summary>
    /// class:      序列节点-扩展：如果是行为节点，只需要在进入之前判断一次前提条件
    /// Evaluate:   若是从头开始的，则调用第一个子节点的Evaluate方法，将其返回值作为自身的返回值返回。否则，调用当前运行节点的Evaluate方法，将其返回值作为自身的返回值返回。
    /// Tick:       调用可以运行的子节点的Tick方法，若返回运行结束，则将下一个子节点作为当前运行节点，若当前已是最后一个子节点，表示该序列已经运行结束，则自身返回运行结束。
    ///             若子节点返回运行中，则用它所返回的运行状态作为自身的运行状态返回
    /// </summary>
    public class BTreeNodeSequence_EvaluateOnce : BTreeNodeCtrl
    {
        private int m_CurrentNodeIndex = INVALID_CHILD_NODE_INDEX;
        public BTreeNodeSequence_EvaluateOnce()
            : base()
        {
        }
        public BTreeNodeSequence_EvaluateOnce(BTreeNode _parentNode, BTreeNodePrecondition _precondition = null)
            : base(_parentNode, _precondition)
        {
        }

        protected override bool _DoEvaluate(BTreeTemplateData _input)
        {
            base._DoEvaluate(_input);
            int testNode;
            if (m_CurrentNodeIndex == INVALID_CHILD_NODE_INDEX)
            {
                testNode = 0;
            }
            else
            {
                testNode = m_CurrentNodeIndex;
            }
            if (_CheckIndex(testNode))
            {
                BTreeNode bn = m_ChildNodes[testNode];
                //判断如果是行为节点，只需要在进入之前判断一次前提条件。
                if (bn is BTreeNodeAction && ((BTreeNodeAction)bn).Status != BTreeActionNodeStatus.Ready)
                {
                    return true;
                }
                else if (bn.Evaluate(_input))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void _DoTransition(BTreeTemplateData _input)
        {
            base._DoTransition(_input);
            if (_CheckIndex(m_CurrentNodeIndex))
            {
                BTreeNode bn = m_ChildNodes[m_CurrentNodeIndex];
                bn.Transition(_input);
            }
            m_CurrentNodeIndex = INVALID_CHILD_NODE_INDEX;
        }

        protected override BTreeRunningStatus _DoTick(BTreeTemplateData _input, ref BTreeTemplateData _output)
        {
            base._DoTick(_input, ref _output);
            BTreeRunningStatus runningStatus = BTreeRunningStatus.Finish;
            //First Time
            if (m_CurrentNodeIndex == INVALID_CHILD_NODE_INDEX)
            {
                m_CurrentNodeIndex = 0;
            }
            BTreeNode bn = m_ChildNodes[m_CurrentNodeIndex];
            runningStatus = bn.Tick(_input, ref _output);
            if (runningStatus == BTreeRunningStatus.Finish)
            {
                m_CurrentNodeIndex++;
                if (m_CurrentNodeIndex < m_ChildCount)
                {
                    runningStatus = BTreeRunningStatus.Executing;
                }
            }
            return runningStatus;
        }
        protected override void _DoReset()
        {
            base._DoReset();
            if (m_CurrentNodeIndex == m_ChildCount)
            {
                m_CurrentNodeIndex = INVALID_CHILD_NODE_INDEX;
            }
        }
    }
}
