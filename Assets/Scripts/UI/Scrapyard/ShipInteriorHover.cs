﻿using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace StarSalvager.UI.Scrapyard
{
    public class ShipInteriorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required] private RectTransform hoverWindowRectTransform;

        [SerializeField, Required] private TMP_Text hoverText;

        [SerializeField] private string displayTitle;

        [FormerlySerializedAs("_offset")] [SerializeField]
        private Vector2 offset;

        private bool _trackingMouse;

        //Unity Functions
        //====================================================================================================================//

        private void Update()
        {
            if (!_trackingMouse)
                return;

            var bounds = new Vector2(Screen.width / 2f, Screen.height / 2f);

            //TODO Need to clamp to screen bounds
            var newPosition = Input.mousePosition - (Vector3) bounds + (Vector3) offset;

            //--------------------------------------------------------------------------------------------------------//

            var sizeDelta = hoverWindowRectTransform.sizeDelta;


            if (newPosition.y > bounds.y)
            {
                newPosition.y = bounds.y + offset.y;
            }
            else if (newPosition.y - sizeDelta.y < -bounds.y)
            {
                newPosition.y = -bounds.y + sizeDelta.y - offset.y;
            }

            if (newPosition.x + sizeDelta.x > bounds.x)
            {
                newPosition.x = bounds.x - sizeDelta.x - offset.x;
            }
            else if (newPosition.x < -bounds.x)
            {
                newPosition.x = -bounds.x + offset.x;
            }

            //--------------------------------------------------------------------------------------------------------//

            hoverWindowRectTransform.anchoredPosition = newPosition;
        }

        private void OnDisable()
        {
            hoverWindowRectTransform.gameObject.SetActive(false);
            _trackingMouse = false;
        }

        //Point event Functions
        //====================================================================================================================//

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverWindowRectTransform.gameObject.SetActive(true);

            hoverWindowRectTransform.position = eventData.position;
            hoverText.text = displayTitle;

            _trackingMouse = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverWindowRectTransform.gameObject.SetActive(false);
            _trackingMouse = false;
        }

        //====================================================================================================================//

    }
}