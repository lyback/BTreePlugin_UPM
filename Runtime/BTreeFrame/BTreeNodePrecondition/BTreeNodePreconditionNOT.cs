  
namespace BTreeFrame
{
    public class BTreeNodePreconditionNOT : BTreeNodePrecondition
    {
        public BTreeNodePrecondition m_Precondition;
        public BTreeNodePreconditionNOT() { }
        public BTreeNodePreconditionNOT(BTreeNodePrecondition _precondition)
        {
            if (_precondition == null)
            {
                UnityEngine.Debug.Log("BTreeNodePreconditionNOT is null");
            }
            m_Precondition = _precondition;
        }
        public override bool ExternalCondition(BTreeTemplateData _input)
        {
            return !m_Precondition.ExternalCondition(_input);
        }
        public BTreeNodePrecondition GetChildPrecondition()
        {
            return m_Precondition;
        }
        public void SetChildPrecondition(BTreeNodePrecondition _precondition)
        {
            m_Precondition = _precondition;
        }
    }
}
