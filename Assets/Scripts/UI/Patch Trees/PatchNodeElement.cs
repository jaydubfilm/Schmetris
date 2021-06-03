﻿using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard.PatchTrees
{
    public class PatchNodeElement : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        [SerializeField] private Button button;

        [SerializeField, BoxGroup("Prototyping")]
        private Sprite patchSprite;
        
        public void Init(in PART_TYPE partType)
        {
            image.sprite = partType.GetSprite();
            button.enabled = false;

        }

        public void Init(in PART_TYPE partType, in PatchData patchData, bool unlocked)
        {
            image.sprite = patchSprite;
            image.color = partType.GetCategory().GetColor();

            button.interactable = unlocked;
        }
    }
}