using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Interfaces;
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
        [SerializeField, Required]
        private TMP_Text hintText;
        [SerializeField, Required]
        private TMP_Text infoText;
        
        [SerializeField, Required]
        private HighlightManager highlightManager;

        //HintManager Functions
        //====================================================================================================================//
        
        public void TryShowHint(HINT hint)
        {
            var hintText = string.Empty;
            var descriptionText = string.Empty;
            //TODO Need to check for show conditions
            switch (hint)
            {
                case HINT.NONE:
                    highlightManager.SetActive(false);
                    break;
                case HINT.MAGNET:
                    break;
                case HINT.BONUS:
                    break;
                case HINT.GUN:
                    hintText = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.GUN).name;
                    descriptionText = "This is a gun part, it makes the pew pew";
                    
                    var gunPart = LevelManager.Instance
                        .BotObject
                        .attachedBlocks
                        .FirstOrDefault(x => x is Part part && part.Type == PART_TYPE.GUN) as Part;
                    
                    highlightManager.Highlight(gunPart);
                    break;
                case HINT.FUEL:
                    break;
                case HINT.HOME:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }

            this.hintText.text = hintText;
            infoText.text = descriptionText;

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
