using UnityEngine;

namespace StarSalvager
{
    public interface IAttachable
    {
        Vector2Int Coordinate { get; set; }
        Transform transform { get; }
        GameObject gameObject { get; }

        bool Attached { get; set; }
        bool CountAsConnectedToCore { get; }
        bool CanDisconnect { get; }
        bool CanShift { get; }

        bool CountTowardsMagnetism { get; }

        void SetAttached(bool isAttached);
    }
}



