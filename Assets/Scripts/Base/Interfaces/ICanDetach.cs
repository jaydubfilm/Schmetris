using UnityEngine;

namespace StarSalvager
{
    public interface ICanDetach
    {
        IAttachable iAttachable { get; }
        GameObject gameObject { get;}
        Transform transform { get; }
        Vector2Int Coordinate { get; }
        
        int AttachPriority { get; }
        bool PendingDetach { get; set; }
        
        void SetAttached(bool isAttached);
    }
}
