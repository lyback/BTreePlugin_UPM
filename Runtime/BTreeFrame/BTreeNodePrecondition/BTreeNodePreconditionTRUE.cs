
namespace BTreeFrame
{
    public class BTreeNodePreconditionTRUE : BTreeNodePrecondition
    {
        public BTreeNodePreconditionTRUE() { }
        public override bool ExternalCondition(BTreeTemplateData _input)
        {
            return true;
        }
    }
}