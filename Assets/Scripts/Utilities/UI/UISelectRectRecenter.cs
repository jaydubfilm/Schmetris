using StarSalvager.Utilities.Inputs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    public class UISelectRectRecenter : MonoBehaviour, ISelectHandler
    {
        private ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                    _scrollRect = GetComponentInParent<ScrollRect>();

                return _scrollRect;
            }
        }

        private ScrollRect _scrollRect;

        private RectTransform ScrollRectArea
        {
            get
            {
                if (_scrollRectArea == null)
                    _scrollRectArea = ScrollRect.content;

                return _scrollRectArea;
            }
        }

        private RectTransform _scrollRectArea;

        private RectTransform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.transform as RectTransform;

                return _transform;
            }
        }

        private RectTransform _transform;

        //TODO I should incorporate options for how the scroll reposition
        //TODO I should base the below calculations on the anchor setup
        private void CenterToItem()
        {
            var localPos = ScrollRectArea.InverseTransformPoint(transform.position);
            var rect = ScrollRectArea.rect;
            
            ScrollRect.horizontalNormalizedPosition = localPos.x / rect.width;
            ScrollRect.verticalNormalizedPosition = 0.9f - Mathf.Abs(-localPos.y / rect.height);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!InputManager.Instance.UsingController)
                return;

            CenterToItem();
        }
    }
}
