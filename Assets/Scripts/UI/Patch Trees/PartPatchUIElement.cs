using System;
using System.Collections.Generic;
using System.Linq;
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

        //====================================================================================================================//

        public readonly struct PatchPartSelectables
        {
            public readonly Selectable[] Selectables;
            public readonly Selectable RightMostSelectable;

            public PatchPartSelectables(in IEnumerable<Selectable> selectables, in Selectable rightMostSelectable)
            {
                Selectables = selectables.ToArray();
                RightMostSelectable = rightMostSelectable;
            }
        }

        public PatchPartSelectables Selectables { get; private set; }

        //====================================================================================================================//

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

        public new RectTransform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.transform as RectTransform;

                return _transform;
            }
        }
        private RectTransform _transform;

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

            PatchOptionUIElement CreatePatchOption(in PART_TYPE type, in PatchData patchData)
            {
                var temp = Instantiate(patchOptionPrefab, patchOptionsContainer, false);
                temp.Init(type, patchData, onPatchSelected, onPatchHovered);

                return temp;
            }

            //--------------------------------------------------------------------------------------------------------//

            _data = partType;

            partButton.onClick.AddListener(() =>
            {
                onPartSelected?.Invoke(_data);
            });

            partButtonImage.sprite = partType.GetSprite();
            //PartAttachableFactory.CreateUIPartBorder((RectTransform)partButtonImage.transform, partType);

            //Used to contain the navigational information for this UIElement
            var outList = new List<Selectable>();
            Selectable rightMost = null;

            for (var i = 0; i < patches.Count; i++)
            {
                var patchData = patches[i];
                var patchOption = CreatePatchOption(partType, patchData);

                outList.Add(patchOption.Button);
                if (i == patches.Count - 1) rightMost = patchOption.Button;
            }

            //Set the current Selectables
            Selectables = new PatchPartSelectables(outList, rightMost);

            LayoutRebuilder.MarkLayoutForRebuild(patchOptionsContainer);
        }
    }
}
