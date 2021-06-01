using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.PatchTrees.Data;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard.PatchTrees
{
    public class PatchTreeUI : MonoBehaviour
    {

        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Prefabs")]
        private GameObject tierElementPrefab;
        [SerializeField, Required, BoxGroup("Prefabs")]
        private GameObject patchTreeElementPrefab;

        [SerializeField, Required]
        private RectTransform patchTreeTierContainer;

        private RectTransform[] _activeTiers;
        private RectTransform[] _activeElements;

        //====================================================================================================================//

        [Button, DisableInEditorMode]
        private void TestGunPatchTree()
        {
            GeneratePatchTree(PART_TYPE.GUN);
        }
        
        private void GeneratePatchTree(in PART_TYPE partType)
        {
            //Instantiate Functions
            //--------------------------------------------------------------------------------------------------------//
            
            RectTransform CreateTierElement()
            {
                var temp = Instantiate(tierElementPrefab, patchTreeTierContainer, false).transform;
                temp.SetSiblingIndex(0);

                return (RectTransform) temp;
            }

            RectTransform CreatePatchElement(in RectTransform container, in PatchData patchData)
            {
                var temp = Instantiate(patchTreeElementPrefab, container, false).transform;
                //TODO Fill with patchData
                
                return (RectTransform) temp;
            }
            
            //--------------------------------------------------------------------------------------------------------//
            
            CleanPatchTree();
            
            var patchTreeData = partType.GetPatchTree();
            var maxTier = patchTreeData.Max(x => x.Tier);
            
            //Add one to account for the part Tier
            _activeTiers = new RectTransform[maxTier + 1];
            //Add one to account for the part Element
            _activeElements = new RectTransform[patchTreeData.Count + 1];

            //Create the base Part Tier
            _activeTiers[0] = CreateTierElement();
            _activeElements[0] = CreatePatchElement(_activeTiers[0], new PatchData());

            //Create the remaining upgrade Tiers
            for (var i = 1; i <= maxTier; i++)
            {
                _activeTiers[i] = CreateTierElement();
            }

            //Populate with Elements
            for (int i = 0; i < patchTreeData.Count; i++)
            {
                var tier = patchTreeData[i].Tier;
                _activeElements[i + 1] = CreatePatchElement(_activeTiers[tier], new PatchData());
                
            }

            //TODO Connect Elements
        }

        private void CleanPatchTree()
        {
            if (_activeTiers.IsNullOrEmpty())
                return;
            
            for (int i = _activeTiers.Length - 1; i >= 0; i--)
            {
                Destroy(_activeTiers[i].gameObject);
            }
        }

        //====================================================================================================================//
        
    }
}
