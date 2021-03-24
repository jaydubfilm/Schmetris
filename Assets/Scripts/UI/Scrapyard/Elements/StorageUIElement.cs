using StarSalvager.Utilities.JsonDataTypes;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class StorageUIElement : ButtonReturnUIElement<TEST_Storage, TEST_Storage>, IPointerEnterHandler, IPointerExitHandler
    {
        private static DroneDesignUI _droneDesignUI;
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

            if (!_droneDesignUI)
                _droneDesignUI = FindObjectOfType<DroneDesignUI>();

            this.data = data;

            itemImage.sprite = data.sprite;
            itemImage.color = data.color;

            var isPart = data.blockData is PartData;

            //Only want to be able to select parts
            button.interactable = isPart && _scrapyardBot != null && !_scrapyardBot.AtPartCapacity;

            if (!button.interactable)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(data);
            });
        }

        //============================================================================================================//

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!(data.blockData is PartData partData))
                return;

            _droneDesignUI.ShowPartDetails(true, partData, transform);
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!(data.blockData is PartData))
                return;

            _droneDesignUI.ShowPartDetails(false, new PartData(), null);
        }

        //============================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            if (partDragImageTransform != null && partDragImageTransform.gameObject != null)
                Destroy(partDragImageTransform.gameObject);

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
