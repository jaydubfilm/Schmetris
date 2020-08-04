using System;
using System.Collections.Generic;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class PartBitImageUIElement : ButtonReturnUIElement<RemoteDataBase, (Enum, int)>, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private static PartAttachableFactory _partAttachableFactory;
        private static BitAttachableFactory _bitAttachableFactory;

        private int level = 0;

        //============================================================================================================//

        [SerializeField, Required]
        private Image logoImage;

        private RectTransform _canvasTr;
        private RectTransform partDragImageTransform;

        //============================================================================================================//

        public void Init(RemoteDataBase data, Action<(Enum, int)> OnPressed, int level)
        {
            this.level = level;
            Init(data, OnPressed);
        }

        public override void Init(RemoteDataBase data, Action<(Enum, int)> OnPressed)
        {
            if (_partAttachableFactory == null)
                _partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            this.data = data;

            if (this.data is PartRemoteData partRemote)
            {
                Debug.Log(partRemote.partType + " --- " + level);
                logoImage.sprite = _partAttachableFactory.GetProfileData(partRemote.partType).Sprites[level];

                button.onClick.AddListener(() =>
                {
                    OnPressed?.Invoke((partRemote.partType, level));
                });
            }
            else if (this.data is BitRemoteData bitRemote)
            {
                logoImage.sprite = _bitAttachableFactory.GetBitProfile(bitRemote.bitType).Sprites[level];

                button.onClick.AddListener(() =>
                {
                    OnPressed?.Invoke((bitRemote.bitType, level));
                });
            }
            else
            {
                Debug.LogError("PartBitImageUIElement Passed in value that is not PartRemoteData");
            }
        }

        //============================================================================================================//

        public void OnBeginDrag(PointerEventData eventData)
        {
            _canvasTr = GetComponentInParent<Canvas>()?.transform as RectTransform;

            Debug.Log($"Canvas: {_canvasTr.gameObject.name}", _canvasTr.gameObject);

            if (partDragImageTransform == null)
            {
                var image = new GameObject("Test").AddComponent<Image>();
                image.sprite = logoImage.sprite;

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
    }
}


