using System;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard.PatchTrees
{
    public class PatchOptionUIElement : MonoBehaviour
    {
        [SerializeField]
        private Button patchButton;

        private PART_TYPE _partType;
        private PatchData _data;

        public void Init(in PART_TYPE partType, in PatchData patchData, Action<PART_TYPE, PatchData> onPatchSelected)
        {
            _partType = partType;
            _data = patchData;
            
            patchButton.onClick.AddListener(() =>
            {
                onPatchSelected?.Invoke(_partType, _data);
            });
            
        }
    }
}
