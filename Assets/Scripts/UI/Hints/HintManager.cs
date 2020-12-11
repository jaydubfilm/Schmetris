using System;
using System.Collections;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Input = UnityEngine.Input;

namespace StarSalvager.UI.Hints
{
    public enum HINT
    {
        NONE,
        MAGNET,
        BONUS,
        GUN,
        FUEL,
        HOME,
        PART,
        PATCH_POINT,
        COMPONENT
    }
    
    [RequireComponent(typeof(HighlightManager))]
    public class HintManager : Singleton<HintManager>
    {
        public static bool USE_HINTS = true;
        
        [SerializeField, Required]
        private HintRemoteDataScriptableObject hintRemoteData;
        
        [SerializeField, Required, Space(10f)]
        private TMP_Text hintText;
        [SerializeField, Required]
        private TMP_Text infoText;

        [SerializeField, Required, Space(10f)]
        private Button confirmButton;
        
        [SerializeField, Required]
        private HighlightManager highlightManager;

        private bool _waiting;
        private string _previousInputActionGroup;

        //Unity Functions
        //====================================================================================================================//

        private void Start()
        {
            confirmButton.onClick.AddListener(StopShowingHint);
        }

        private void Update()
        {
            if (!_waiting)
                return;
            
            //FIXME incorporate the new input system here
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopShowingHint();
            }
            
        }

        //HintManager Functions
        //====================================================================================================================//
        
        public static void TryShowHint(HINT hint)
        {
            if (!USE_HINTS)
                return;

            //Don't want to show the hints while Im doing tutorial
            if (Globals.UsingTutorial)
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

                    if (bonusShape is IRecycled recycled && recycled.IsRecycled)
                        return;
                    
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
                    var hasBits = PlayerDataManager.GetBlockDatas().Any(x => x.ClassType.Equals(nameof(Bit)));
                    if (!hasBits)
                        return;
                    
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

            _previousInputActionGroup = InputManager.CurrentActionMap;
            InputManager.SwitchCurrentActionMap("Menu Controls");

        }

        private void ShowHintData(HintRemoteDataScriptableObject.HintData hintData)
        {
            hintText.text = hintData.shortText;
            infoText.text = hintData.longDescription;
        }

        private void StopShowingHint()
        {
            if (!_waiting)
                return;
            
            InputManager.SwitchCurrentActionMap(_previousInputActionGroup);
            _previousInputActionGroup = string.Empty;
            
            Time.timeScale = 1f;
            highlightManager.SetActive(false);
            _waiting = false;
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
