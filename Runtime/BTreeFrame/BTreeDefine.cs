  
namespace BTreeFrame
{
    
    //行为树运行状态
    public enum BTreeRunningStatus
    {
        Error = 1,
        Executing = 2,
        Finish = 3,
    }
    //行为树Action节点运行状态
    public enum BTreeActionNodeStatus
    {
        Ready = 1,
        Running = 2,
        Finish = 3,
    }
    public enum BTreeParallelFinishCondition
    {
        OR = 1,
        AND = 2,
    }

}
