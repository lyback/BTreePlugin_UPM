
namespace BTreeFrame
{
    //选择节点基类
    public class BTreeNodeCtrl : BTreeNode
    {
        public bool ResetOnFinish = true;
        public BTreeNodeCtrl()
            : base()
        {

        }
        public BTreeNodeCtrl(BTreeNode _parentNode, BTreeNodePrecondition _precondition = null)
            : base(_parentNode, _precondition)
        {

        }

        public override BTreeRunningStatus Tick(BTreeTemplateData _input, ref BTreeTemplateData _output)
        {
            var runningstatus = base.Tick(_input, ref _output);
            if (runningstatus == BTreeRunningStatus.Finish)
            {
                if (ResetOnFinish)
                {
                    _DoReset();
                }
            }
            else if (runningstatus == BTreeRunningStatus.Error)
            {
                _DoReset();
            }
            return runningstatus;
        }

        protected virtual void _DoReset()
        {

        }
    }
}
