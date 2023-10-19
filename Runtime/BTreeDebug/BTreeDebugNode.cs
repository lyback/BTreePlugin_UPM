
namespace BTree.Editor
{
    public enum BTreeDebugNodeState
    {
        None = 0,
        Ready = 1,
        Running = 2,
        Finish = 3,
        Error = 4,
    }
    public class BTreeDebugNode
    {
        public int[] indexs;
        public BTreeDebugNodeState state = BTreeDebugNodeState.None;
        public float progress = 0;
        public BTreeDebugNode()
        {
        }
        public bool IsSelf(int[] _indexs)
        {
            if (indexs.Length == _indexs.Length)
            {
                for (int i = 0; i < indexs.Length; i++)
                {
                    if (indexs[i] != (int)_indexs[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public void SetIndexs(int[] _indexs)
        {
            indexs = _indexs;
        }
        public virtual void SetData(object param)
        {

        }
    }
}