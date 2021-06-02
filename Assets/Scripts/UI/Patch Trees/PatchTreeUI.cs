using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.PatchTrees.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.UI;
using UnityEngine;
using UnityEngine.UI;

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
        private RectTransform[] _activeElementLinks;
        private RectTransform _lineContainer;

        //====================================================================================================================//

        [Button, DisableInEditorMode]
        private void TestGunPatchTree()
        {
            //GeneratePatchTree(PART_TYPE.GUN);
            StartCoroutine(GeneratePatchTreeCoroutine(PART_TYPE.GUN));
        }
        
        private IEnumerator GeneratePatchTreeCoroutine(PART_TYPE partType)
        {
            yield return new WaitForSeconds(1f);
            
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

            RectTransform CreateUILine(in RectTransform startTransform, in RectTransform endTransform)
            {
                if (_lineContainer == null)
                {
                    var temp = new GameObject("Line Container");
                    var layoutElement = temp.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;

                    _lineContainer = (RectTransform)temp.transform;
                    _lineContainer.SetParent(patchTreeTierContainer, false);
                    _lineContainer.SetSiblingIndex(0);
                }

                var image = UILineCreator.DrawConnection(_lineContainer, startTransform, endTransform, Color.white);
                return image.transform as RectTransform;
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

            //--------------------------------------------------------------------------------------------------------//
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(patchTreeTierContainer);
            
            //Wait one frame while elements reposition before drawing the lines
            yield return null;
            
            //Lines
            //--------------------------------------------------------------------------------------------------------//
            
            var activeLinks = new List<RectTransform>();
            //Connect Elements
            for (var i = 0; i < patchTreeData.Count; i++)
            {
                var endIndex = i + 1;
                var endElement = _activeElements[endIndex];

                //var patchNode = patchTreeData[i];
                var links = patchTreeData[i].PreReqs;

                if (links.IsNullOrEmpty())
                {
                    //Create a line between this node and the part
                    activeLinks.Add(CreateUILine(_activeElements[0], endElement));
                    continue;
                }

                for (var j = 0; j < links.Length; j++)
                {
                    var startIndex = links[j] + 1;
                    var startElement = _activeElements[startIndex];

                    activeLinks.Add(CreateUILine(startElement, endElement));
                }
                
            }

            _activeElementLinks = activeLinks.ToArray();

            //--------------------------------------------------------------------------------------------------------//
            
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
