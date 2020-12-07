using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Hints
{
    public enum HINT
    {
        NONE,
        MAGNET,
        BONUS,
        GUN,
        FUEL,
        HOME
    }
    
    [RequireComponent(typeof(HighlightManager))]
    public class HintManager : Singleton<HintManager>
    {
        private Dictionary<HINT, bool> _usedHints;
        
        [SerializeField, Required]
        private TMP_Text hintText;
        [SerializeField, Required]
        private TMP_Text infoText;
        
        [SerializeField, Required]
        private HighlightManager highlightManager;

        private bool _waiting;

        //Unity Functions
        //====================================================================================================================//

        private void Start()
        {
            _usedHints = new Dictionary<HINT, bool>
            {
                [HINT.MAGNET] = false,
                [HINT.BONUS] = false,
                [HINT.GUN] = false,
                [HINT.FUEL] = false,
                [HINT.HOME] = false,
            };
        }

        private void Update()
        {
            if (!_waiting)
                return;
            
            //FIXME incorporate the new input system here
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = 1f;
                highlightManager.SetActive(false);
                _waiting = false;
            }
            
        }

        //HintManager Functions
        //====================================================================================================================//
        
        public void TryShowHint(HINT hint)
        {
            if (hint != HINT.NONE && _usedHints[hint])
                return;
            
            var hintText = string.Empty;
            var descriptionText = string.Empty;
            //TODO Need to check for show conditions
            switch (hint)
            {
                //----------------------------------------------------------------------------------------------------//
                case HINT.NONE:
                    highlightManager.SetActive(false);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.MAGNET:
                    hintText = "Magnetism";
                    descriptionText = "Can only connect bits until this meter is full. Make combos to compress bits";
                    
                    var magnetSlider = GameUI.Instance.GetHintElement(hint);
                    highlightManager.Highlight(magnetSlider);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.BONUS:
                    hintText = "Bonus Shape";
                    descriptionText = "Upgrade bits using Bonus Shapes";
                    
                    var bonusShape = FindObjectOfType<ObstacleManager>().ActiveBonusShapes.FirstOrDefault();
                    
                    highlightManager.Highlight(bonusShape);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.GUN:
                    hintText = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.GUN).name;
                    descriptionText = "This is a gun part, it makes the pew pew";
                    
                    var gunPart = LevelManager.Instance
                        .BotObject
                        .attachedBlocks
                        .FirstOrDefault(x => x is Part part && part.Type == PART_TYPE.GUN) as Part;
                    
                    highlightManager.Highlight(gunPart);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.FUEL:
                    hintText = "Fuel";
                    descriptionText = "Amount of Fuel left. Gather more by collecting red bits";
                    
                    var fuelSlider = GameUI.Instance.GetHintElement(hint);
                    highlightManager.Highlight(fuelSlider);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.HOME:
                    hintText = "Shipwreck";
                    descriptionText = "Click here when you're ready to return to base to regroup, craft parts, and prepare to return to the fields.";
                    
                    var homeButton = FindObjectOfType<UniverseMap>().GetHintElement(hint);
                    highlightManager.Highlight(homeButton);
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
                //----------------------------------------------------------------------------------------------------//
            }

            _usedHints[hint] = true;

            this.hintText.text = hintText;
            infoText.text = descriptionText;

            _waiting = true;
            
            Time.timeScale = 0f;
            
        }

        //Editor Functions
        //====================================================================================================================//

#if UNITY_EDITOR

        [Button]
        private void TestGunHighlight()
        {
            TryShowHint(HINT.GUN);
        }
        
#endif
        
    }
}
