using StarSalvager.Values;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollSpeedAdjuster : MonoBehaviour
    {
        private ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                    _scrollRect = GetComponent<ScrollRect>();
                
                return _scrollRect;
            }
        }
        private ScrollRect _scrollRect;

        private void OnEnable()
        {
#if UNITY_STANDALONE_WIN
            ScrollRect.scrollSensitivity = Globals.WindowsScrollSpeed;
#elif UNITY_STANDALONE_OSX
            ScrollRect.scrollSensitivity = Globals.MacOSScrollSpeed;
#endif
        }
    }
}
