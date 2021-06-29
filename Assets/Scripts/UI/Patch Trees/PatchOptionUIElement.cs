using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard.PatchTrees
{
    public class PatchOptionUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private Action<RectTransform, PatchData, bool> _onPatchHovered;
        
        [SerializeField]
        private Button patchButton;

        private PART_TYPE _partType;
        private PatchData _data;

        public void Init(in PART_TYPE partType, in PatchData patchData, 
            Action<PART_TYPE, PatchData> onPatchSelected,
            Action<RectTransform, PatchData, bool> onPatchHovered)
        {
            _partType = partType;
            _data = patchData;
            
            patchButton.onClick.AddListener(() =>
            {
                onPatchSelected?.Invoke(_partType, _data);
            });

            _onPatchHovered = onPatchHovered;

        }

        //IPointerHandle Functions
        //====================================================================================================================//
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onPatchHovered?.Invoke((RectTransform)patchButton.transform, _data, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onPatchHovered?.Invoke(null, default, false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _onPatchHovered?.Invoke((RectTransform)patchButton.transform, _data, true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _onPatchHovered?.Invoke(null, default, false);
        }
    }
}
