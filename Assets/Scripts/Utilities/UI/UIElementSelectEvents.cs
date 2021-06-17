using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace StarSalvager.Utilities.UI
{
    public class UIElementSelectEvents : MonoBehaviour, ISelectHandler, IDeselectHandler
    {

        [SerializeField]
        private Color color = Color.white;
        
        private new RectTransform transform
        {
            get
            {
                if (_transform == null)
                    _transform = (RectTransform)gameObject.transform;

                return _transform;
            }
        }
        private RectTransform _transform;
        
        //====================================================================================================================//

        public void OnSelect(BaseEventData eventData)
        {
            Debug.Log($"Select {gameObject.name}");
            UISelectHandler.OutlineObject(transform, color);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Debug.Log($"Deselect {gameObject.name}");
            UISelectHandler.OutlineObject(null);
        }

        //====================================================================================================================//
        
    }

}