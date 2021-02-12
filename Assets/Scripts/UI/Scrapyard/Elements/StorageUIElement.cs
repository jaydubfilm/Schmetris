using StarSalvager.Utilities.JsonDataTypes;
using System;
using StarSalvager.Cameras;
using StarSalvager.Factories;
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

        private Image _damageImage;

        //============================================================================================================//

        public override void Init(TEST_Storage data, Action<TEST_Storage> onPressedCallback)
        {
            if (!_scrapyardBot)
                _scrapyardBot = FindObjectOfType<ScrapyardBot>();

            this.data = data;

            itemImage.sprite = data.sprite;
            itemImage.color = data.color;

            var isPart = data.blockData.ClassType == nameof(Part) ||
                         data.blockData.ClassType == nameof(ScrapyardPart);

            //Only want to be able to select parts
            button.interactable = isPart && _scrapyardBot != null && !_scrapyardBot.AtPartCapacity;

            if (!button.interactable)
                return;

            /*if (isPart)
                SetupDamageSprite();*/

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(data);
            });
        }

        //============================================================================================================//

        /*private void SetupDamageSprite()
        {
            /*var maxHealth = FactoryManager.Instance.PartsRemoteData.GetRemoteData((PART_TYPE) data.blockData.Type)
                .levels[data.blockData.Level].health;
            //Add Damage Overlay
            var healthValue = data.blockData.Health / maxHealth;

            if (healthValue == 0f)
            {
                itemImage.sprite = FactoryManager.Instance.PartsProfileData.GetDamageSprite(data.blockData.Level);
                return;
            }

            var sprite = FactoryManager.Instance.DamageProfile.GetDetailSprite(healthValue);

            if (sprite == null)
                return;

            var temp = Instantiate(itemImage, itemImage.transform, false);
            ((RectTransform) temp.transform).sizeDelta = Vector2.zero;

            _damageImage = temp;
            _damageImage.sprite = sprite;
        }*/

        //====================================================================================================================//


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
                image.color = data.color;

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

            if(_damageImage)
                Destroy(_damageImage.gameObject);
        }
    }

    public class TEST_Storage : IEquatable<TEST_Storage>
    {
        public string name;
        public Sprite sprite;
        public Color color;
        public IBlockData blockData;
        public int storageIndex;

        public bool Equals(TEST_Storage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && blockData.Equals(other.blockData) && storageIndex == other.storageIndex;
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
