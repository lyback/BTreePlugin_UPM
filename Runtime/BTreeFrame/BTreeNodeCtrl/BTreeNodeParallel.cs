using System.Collections.Generic;
using UnityEngine;
namespace BTreeFrame
{
    /// <summary>
    /// class:      并行节点
    /// Evaluate:   依次调用所有的子节点的Evaluate方法，若所有的子节点都返回True，则自身也返回True，否则，返回False
    /// Tick:       调用所有子节点的Tick方法，若并行节点是“或者”的关系，则只要有一个子节点返回运行结束，那自身就返回运行结束。
    ///             若并行节点是“并且”的关系，则只有所有的子节点返回结束，自身才返回运行结束
    /// </summary>
    public class BTreeNodeParalle : BTreeNodeCtrl
    {
        [Header("完成条件")]
        public BTreeParallelFinishCondition FinishCondition = BTreeParallelFinishCondition.AND;
        private List<BTreeRunningStatus> m_ChildNodeSatuses = new List<BTreeRunningStatus>();
        public BTreeNodeParalle()
            : base()
        {
        }
        public BTreeNodeParalle(BTreeNode _parentNode, BTreeNodePrecondition _precondition = null)
            : base(_parentNode, _precondition)
        {
        }

        protected override bool _DoEvaluate(BTreeTemplateData _input)
        {
            base._DoEvaluate(_input);
            for (int i = 0; i < m_ChildCount; i++)
            {
                BTreeNode bn = m_ChildNodes[i];
                if (m_ChildNodeSatuses[i] == BTreeRunningStatus.Executing)
                {
                    if (!bn.Evaluate(_input))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        protected override void _DoTransition(BTreeTemplateData _input)
        {
            base._DoTransition(_input);
            for (int i = 0; i < m_ChildCount; i++)
            {
                m_ChildNodeSatuses[i] = BTreeRunningStatus.Executing;
                BTreeNode bn = m_ChildNodes[i];
                bn.Transition(_input);
            }
        }

        protected override BTreeRunningStatus _DoTick(BTreeTemplateData _input, ref BTreeTemplateData _output)
        {
            int finishedChildCount = 0;
            int errorChildCount = 0;

            if (FinishCondition == BTreeParallelFinishCondition.OR)
            {
                for (int i = 0; i < m_ChildCount; i++)
                {
                    BTreeNode bn = m_ChildNodes[i];
                    BTreeRunningStatus status = bn.Tick(_input, ref _output);
                    m_ChildNodeSatuses[i] = status;
                    if (status != BTreeRunningStatus.Executing)
                    {
                        return status;
                    }
                }
            }
            else if (FinishCondition == BTreeParallelFinishCondition.AND)
            {
                for (int i = 0; i < m_ChildCount; i++)
                {
                    BTreeNode bn = m_ChildNodes[i];
                    BTreeRunningStatus status = m_ChildNodeSatuses[i];
                    if (status == BTreeRunningStatus.Executing)
                    {
                        status = bn.Tick(_input, ref _output);
                    }
                    if (status == BTreeRunningStatus.Finish)
                    {
                        finishedChildCount++;
                    }
                    else if (status == BTreeRunningStatus.Error)
                    {
                        errorChildCount++;
                    }
                    m_ChildNodeSatuses[i] = status;
                }
                if ((finishedChildCount + errorChildCount) == m_ChildCount)
                {
                    return errorChildCount > 0 ? BTreeRunningStatus.Error : BTreeRunningStatus.Finish;
                }
            }
            else
            {
                UnityEngine.Debug.LogError("BTreeParallelFinishCondition Error");
                return BTreeRunningStatus.Error;
            }
            return BTreeRunningStatus.Executing;
        }
        protected override void _DoReset()
        {
            base._DoReset();
            for (int i = 0; i < m_ChildCount; i++)
            {
                m_ChildNodeSatuses[i] = BTreeRunningStatus.Executing;
            }
        }
        public override void AddChildNode(BTreeNode _childNode)
        {
            base.AddChildNode(_childNode);
            m_ChildNodeSatuses.Add(BTreeRunningStatus.Executing);
        }

    }
}
