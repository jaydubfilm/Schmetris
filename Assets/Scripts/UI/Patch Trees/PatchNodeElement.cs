using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard.PatchTrees
{
    public class PatchNodeElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public new RectTransform transform => gameObject.transform as RectTransform;

        private Action<RectTransform, PatchData, bool> _onHovered;
        
        [SerializeField]
        private Image image;

        [SerializeField] private Button button;

        [SerializeField, BoxGroup("Prototyping")]
        private Sprite patchSprite;

        public bool Unlocked { get; private set; }

        public PatchData patchData;
        private PART_TYPE _partType;

        public void Init(in PART_TYPE partType)
        {
            image.sprite = partType.GetSprite();
            button.enabled = false;
            Unlocked = true;
        }

        public void Init(in PART_TYPE partType, in PatchData patchData,bool hasPurchased, bool unlocked, Action<PART_TYPE, PatchData> onPressedCallback, Action<RectTransform, PatchData, bool> onPatchHovered)
        {
            _onHovered = onPatchHovered;

            image.sprite = patchSprite;
            image.color = partType.GetCategory().GetColor();

            //If the player has already purchased this patch, show it solid, but not interactable
            button.enabled = !hasPurchased;
            if(!hasPurchased)
                button.interactable = unlocked;
            
            Unlocked = unlocked;

            _partType = partType;
            this.patchData = patchData;

            if (!unlocked)
                return;
            
            button.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(_partType, this.patchData);
            });

        }

        //IPointerEvent Functions
        //====================================================================================================================//
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHovered?.Invoke(transform, patchData, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHovered?.Invoke(null, default, false);
        }
    }
}
