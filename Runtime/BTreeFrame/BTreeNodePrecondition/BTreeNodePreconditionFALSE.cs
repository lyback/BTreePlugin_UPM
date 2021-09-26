
namespace BTreeFrame
{
    public class BTreeNodePreconditionFALSE : BTreeNodePrecondition
    {
        public BTreeNodePreconditionFALSE() { }
        public override bool ExternalCondition(BTreeTemplateData _input)
        {
            return false;
        }
    }
}