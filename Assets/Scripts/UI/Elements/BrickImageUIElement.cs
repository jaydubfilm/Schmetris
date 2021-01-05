using System;
using System.Collections.Generic;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class BrickImageUIElement : ButtonReturnUIElement<RemoteDataBase, (Enum, int)>, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private static PartAttachableFactory _partAttachableFactory;
        private static BitAttachableFactory _bitAttachableFactory;

        private int _level;

        //============================================================================================================//

        [SerializeField, Required]
        private Image logoImage;

        private RectTransform _canvasTr;
        private RectTransform partDragImageTransform;

        //============================================================================================================//

        public void Init(RemoteDataBase data, Action<(Enum, int)> onPressedCallback, int level)
        {
            _level = level;
            Init(data, onPressedCallback);
        }

        public override void Init(RemoteDataBase data, Action<(Enum, int)> onPressedCallback)
        {
            if (_partAttachableFactory == null)
                _partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            this.data = data;

            if (this.data is PartRemoteData partRemote)
            {
                logoImage.sprite = _partAttachableFactory.GetProfileData(partRemote.partType).GetSprite();

                button.onClick.AddListener(() =>
                {
                    onPressedCallback?.Invoke((partRemote.partType, _level));
                });
            }
            else if (this.data is BitRemoteData bitRemote)
            {
                logoImage.sprite = _bitAttachableFactory.GetBitProfile(bitRemote.bitType).Sprites[_level];

                button.onClick.AddListener(() =>
                {
                    onPressedCallback?.Invoke((bitRemote.bitType, _level));
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

        public override void CustomRecycle(params object[] args)
        {
            if (partDragImageTransform != null && partDragImageTransform.gameObject != null)
                GameObject.Destroy(partDragImageTransform.gameObject);
        }
    }
}


