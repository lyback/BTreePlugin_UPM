  
namespace BTreeFrame
{
    public abstract class BTreeNodePrecondition
    {
        public BTreeNodePrecondition() { }
        public abstract bool ExternalCondition(BTreeTemplateData _input);
    }

}
