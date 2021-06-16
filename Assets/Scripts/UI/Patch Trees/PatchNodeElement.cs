using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard.PatchTrees
{
    public class PatchNodeElement : MonoBehaviour
    {
        public new RectTransform transform => gameObject.transform as RectTransform;
        
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

        public void Init(in PART_TYPE partType, in PatchData patchData,bool hasPurchased, bool unlocked, Action<PART_TYPE, PatchData> onPressedCallback)
        {
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
    }
}
