using StarSalvager.Utilities.Interfaces;
using UnityEngine;

namespace StarSalvager
{
    public interface IAttachable : IHasBounds
    {
        Vector2Int Coordinate { get; set; }
        Transform transform { get; }
        GameObject gameObject { get; }

        bool Attached { get;}
        bool CountAsConnectedToCore { get; }
        //bool CanDisconnect { get; }
        bool CanShift { get; }

        

        bool CountTowardsMagnetism { get; }

        void SetAttached(bool isAttached);
    }
}



