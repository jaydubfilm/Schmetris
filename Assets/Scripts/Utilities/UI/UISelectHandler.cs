using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    public class UISelectHandler : Singleton<UISelectHandler>
    {
        [SerializeField]
        private Image outlinePrefab;

        private Image _outline;
        private RectTransform _outlineTransform;

        //====================================================================================================================//
        
        public static void OutlineObject(in RectTransform rectTransform)
        {
            Instance.Outline(rectTransform, Color.black);
        }
        public static void OutlineObject(in RectTransform rectTransform, in Color color)
        {
            Instance.Outline(rectTransform, color);
        }

        private void Outline(in RectTransform rectTransform, in Color color)
        {
            if (_outline == null)
            {
                _outline = Instantiate(outlinePrefab);
                _outlineTransform = (RectTransform)_outline.transform;
            }

            if (rectTransform == null)
            {
                _outline.gameObject.SetActive(false);
                return;
            }

            _outline.gameObject.SetActive(true);
            _outlineTransform.SetParent(rectTransform.parent, false);

            var siblingIndex = rectTransform.GetSiblingIndex();
            _outlineTransform.SetSiblingIndex(siblingIndex);

            _outlineTransform.sizeDelta = rectTransform.sizeDelta;
            _outline.color = color;
        }

        //====================================================================================================================//
        
    }
}
