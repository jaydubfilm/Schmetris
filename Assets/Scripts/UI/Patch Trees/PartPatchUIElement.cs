using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard.PatchTrees
{
    public class PartPatchUIElement : MonoBehaviour
    {
        private PART_TYPE _partType;

        [SerializeField, Required]
        private PatchOptionUIElement patchOptionPrefab;
        
        [SerializeField, Required]
        private Button partButton;
        [SerializeField, Required]
        private Image partButtonImage;
        [SerializeField, Required]
        private RectTransform patchOptionsContainer;

        private PART_TYPE _data;

        public void Init(in PartData partData, 
            Action<PART_TYPE> onPartSelected, 
            Action<PART_TYPE, PatchData> onPatchSelected,
            Action<RectTransform, PatchData, bool> onPatchHovered)
        {
            Init((PART_TYPE) partData.Type, partData.Patches, onPartSelected, onPatchSelected, onPatchHovered);
        }
        public void Init(in PART_TYPE partType, in List<PatchData> patches, 
            Action<PART_TYPE> onPartSelected, 
            Action<PART_TYPE, PatchData> onPatchSelected,
            Action<RectTransform, PatchData, bool> onPatchHovered)
        {

            //--------------------------------------------------------------------------------------------------------//
            
            void CreatePatchOption(in PART_TYPE type, in PatchData patchData)
            {
                var temp = Instantiate(patchOptionPrefab, patchOptionsContainer, false);
                temp.Init(type, patchData, onPatchSelected, onPatchHovered);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            _data = partType;
            
            partButton.onClick.AddListener(() =>
            {
                onPartSelected?.Invoke(_data);
            });
            
            partButtonImage.sprite = partType.GetSprite();
            PartAttachableFactory.CreateUIPartBorder((RectTransform)partButtonImage.transform, partType);

            foreach (var patchData in patches)
            {
                CreatePatchOption(partType, patchData);
            }
            
            LayoutRebuilder.MarkLayoutForRebuild(patchOptionsContainer);
        }
    }
}
