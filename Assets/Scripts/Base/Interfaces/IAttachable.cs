using UnityEngine;

namespace StarSalvager
{
    public interface IAttachable
    {
        Vector2Int Coordinate { get; set; }
        Transform transform { get; }
        GameObject gameObject { get; }

        bool Attached { get; set; }

        bool CanShift { get; }

        void SetAttached(bool isAttached);
    }
}



