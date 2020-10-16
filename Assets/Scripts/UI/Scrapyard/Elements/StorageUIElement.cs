﻿using StarSalvager.Utilities.JsonDataTypes;
using System;
using StarSalvager.Cameras;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class StorageUIElement : ButtonReturnUIElement<TEST_Storage, TEST_Storage>, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private static ScrapyardBot _scrapyardBot;

        
        [SerializeField] private Image itemImage;
        private RectTransform _canvasTr;
        private RectTransform partDragImageTransform;
        
        //============================================================================================================//
        
        public override void Init(TEST_Storage data, Action<TEST_Storage> onPressedCallback)
        {
            if (!_scrapyardBot)
                _scrapyardBot = FindObjectOfType<ScrapyardBot>();
            
            this.data = data;

            itemImage.sprite = data.sprite;

            //Only want to be able to select parts
            button.interactable =
                (data.blockData.ClassType == nameof(Part) || data.blockData.ClassType == nameof(ScrapyardPart)) &&
                !_scrapyardBot.AtPartCapacity;
            
            if (!button.interactable)
                return;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(data);
            });
        }
        
        //============================================================================================================//

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            
            _canvasTr = GetComponentInParent<Canvas>()?.transform as RectTransform;
            
            Debug.Log($"Canvas: {_canvasTr.gameObject.name}", _canvasTr.gameObject);
            
            if (partDragImageTransform == null)
            {
                var image = new GameObject("Test").AddComponent<Image>();
                image.sprite = data.sprite;
                
                partDragImageTransform = image.transform as RectTransform;
                partDragImageTransform.anchorMin = partDragImageTransform.anchorMax = Vector2.one * 0.5f;

                partDragImageTransform.SetParent(_canvasTr.transform);
            }

            var cam = FindObjectOfType<CameraController>().GetComponent<Camera>();

            var screenSize = (cam.WorldToScreenPoint(Vector3.right * Constants.gridCellSize) - cam.WorldToScreenPoint(Vector3.zero)).x;
            partDragImageTransform.sizeDelta = Vector2.one * screenSize;


            partDragImageTransform.anchoredPosition = eventData.position - (Vector2)_canvasTr.position;
            partDragImageTransform.gameObject.SetActive(true);
            
            button.onClick.Invoke();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            
            if (partDragImageTransform == null)
                return;

            partDragImageTransform.anchoredPosition = _canvasTr.InverseTransformPoint(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (partDragImageTransform == null)
                return;
            
            partDragImageTransform.gameObject.SetActive(false);
        }
        
        //============================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            if (partDragImageTransform != null && partDragImageTransform.gameObject != null)
                GameObject.Destroy(partDragImageTransform.gameObject);
        }
    }
    
    public class TEST_Storage : IEquatable<TEST_Storage>
    {
        public string name;
        public Sprite sprite;
        public BlockData blockData;

        public bool Equals(TEST_Storage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && blockData.Equals(other.blockData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TEST_Storage) obj);
        }

        public override int GetHashCode()
        {
            return name != null ? name.GetHashCode() : 0;
        }
    }
}

