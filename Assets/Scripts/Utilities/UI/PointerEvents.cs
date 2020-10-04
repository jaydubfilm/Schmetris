using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarSalvager.Utilities.UI
{
    public class PointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<bool> PointerEntered;

        //====================================================================================================================//
        
        private void OnDestroy()
        {
            PointerEntered = null;
        }

        //====================================================================================================================//
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEntered?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerEntered?.Invoke(false);
        }

        //====================================================================================================================//
        
    }
}
