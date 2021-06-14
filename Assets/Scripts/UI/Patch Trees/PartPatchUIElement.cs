using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard.PatchTrees
{
    public class PartPatchUIElement : MonoBehaviour
    {
        private PART_TYPE _partType;

        [SerializeField, Required]
        private PatchOptionUIElement patchOptionPrefab;
        
        [SerializeField, Required]
        private Image partImage;
        [SerializeField, Required]
        private RectTransform patchOptionsContainer;

        public void Init(in PartData partData)
        {
            Init((PART_TYPE) partData.Type, partData.Patches);
        }
        public void Init(in PART_TYPE partType, in List<PatchData> patches)
        {
            void CreatePatchOption(in PatchData patchData)
            {
                var temp = Instantiate(patchOptionPrefab, patchOptionsContainer, false);
                temp.Init(patchData);
            }
            
            partImage.sprite = partType.GetSprite();

            foreach (var patchData in patches)
            {
                CreatePatchOption(patchData);
            }
            
            LayoutRebuilder.MarkLayoutForRebuild(patchOptionsContainer);
        }
    }
}
