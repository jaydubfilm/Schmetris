using UnityEngine;

namespace StarSalvager
{
    public interface ICanBeSeen
    {
        bool IsSeen { get; set; }
        float CameraCheckArea { get; }

        Transform transform { get; }

        void RegisterCanBeSeen();
        void UnregisterCanBeSeen();

        void EnteredCamera();
        void ExitedCamera();
    }

}