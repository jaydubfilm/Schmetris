using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Saving;
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
        public static bool USE_HINTS = true;
        
        [SerializeField, Required]
        private HintRemoteDataScriptableObject hintRemoteData;
        
        //private Dictionary<HINT, bool> _usedHints;
        
        [SerializeField, Required]
        private TMP_Text hintText;
        [SerializeField, Required]
        private TMP_Text infoText;
        
        [SerializeField, Required]
        private HighlightManager highlightManager;

        private bool _waiting;

        //Unity Functions
        //====================================================================================================================//

        /*private void Start()
        {
            _usedHints = new Dictionary<HINT, bool>
            {
                [HINT.MAGNET] = false,
                [HINT.BONUS] = false,
                [HINT.GUN] = false,
                [HINT.FUEL] = false,
                [HINT.HOME] = false,
            };
        }*/

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
        
        public static void TryShowHint(HINT hint)
        {
            if (!USE_HINTS)
                return;
            
            if (Instance == null)
                return;
            
            Instance.ShowHint(hint);
        }

        public static bool CanShowHint(HINT hint)
        {
            if (!USE_HINTS)
                return false;
            
            if (Instance == null)
                return false;

            if (hint == HINT.NONE)
                return false;
            

            return !PlayerDataManager.GetHint(hint);
        }

        public static void TryShowHint(HINT hint, float delayTime)
        {
            if (!USE_HINTS)
                return;
            
            if (Instance == null)
                return;

            Instance.StartCoroutine(WaitCoroutine(delayTime, () =>
            {
                Instance.ShowHint(hint);
            }));
        }
        
        private static IEnumerator WaitCoroutine(float time, Action onWaitCallback)
        {
            yield return new WaitForSeconds(time);
            
            onWaitCallback?.Invoke();
        }
        
        //====================================================================================================================//
        
        private void ShowHint(HINT hint)
        {
            if (hint != HINT.NONE && PlayerDataManager.GetHint(hint))
                return;
            
            switch (hint)
            {
                //----------------------------------------------------------------------------------------------------//
                case HINT.NONE:
                    highlightManager.SetActive(false);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.MAGNET:
                    var magnetSlider = GameUI.Instance.GetHintElement(hint);
                    highlightManager.Highlight(magnetSlider);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.BONUS:
                    var bonusShape = FindObjectOfType<ObstacleManager>().ActiveBonusShapes.FirstOrDefault();
                    
                    highlightManager.Highlight(bonusShape);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.GUN:
                    var gunPart = LevelManager.Instance
                        .BotObject
                        .attachedBlocks
                        .FirstOrDefault(x => x is Part part && part.Type == PART_TYPE.GUN) as Part;
                    
                    highlightManager.Highlight(gunPart);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.FUEL:
                    var fuelSlider = GameUI.Instance.GetHintElement(hint);
                    highlightManager.Highlight(fuelSlider);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.HOME:
                    var homeButton = FindObjectOfType<UniverseMap>().GetHintElement(hint);
                    highlightManager.Highlight(homeButton);
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
                //----------------------------------------------------------------------------------------------------//
            }

            PlayerDataManager.SetHint(hint, true);

            ShowHintData(hintRemoteData.GetHintData(hint));

            _waiting = true;
            
            Time.timeScale = 0f;
            
        }

        private void ShowHintData(HintRemoteDataScriptableObject.HintData hintData)
        {
            hintText.text = hintData.shortText;
            infoText.text = hintData.longDescription;
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
