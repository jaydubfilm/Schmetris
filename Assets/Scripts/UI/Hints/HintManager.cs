using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects.Hints;
using StarSalvager.UI.Wreckyard.PatchTrees;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
        CRAFT_PART,
        GEARS,
        PATCH_POINT,
        COMPONENT,
        PARASITE,
        DAMAGE,
        WHITE,
        SILVER,
        HEALTH,
        WRECK,
        STAR,
        MAP,
        LAYOUT,
        PICK_PART,
        ENTER_WRECK,
        PATCH_TREE
    }
    
    [RequireComponent(typeof(HighlightManager))]
    public class HintManager : Singleton<HintManager>
    {
        public static bool USE_HINTS = true;

        public static Action<bool> OnShowingHintAction;

        public bool ShowingHint { get; private set; }

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

        private ACTION_MAP _previousInputActionGroup;

        //HintManager Functions
        //====================================================================================================================//
        
        public static bool CanShowHint(HINT hint)
        {
            if (!USE_HINTS)
                return false;
            
            if (Instance == null)
                return false;

            //Make sure we're not competing for the screen space
            if (GameManager.IsState(GameState.LevelActiveEndSequence))
                return false;

            if (hint == HINT.NONE)
                return false;
            

            return !PlayerDataManager.GetHint(hint);
        }

        //====================================================================================================================//

        public static void TryShowHint(HINT hint, params object[] objectsToHighlight)
        {
            if (!USE_HINTS)
                return;

            //Don't want to show the hints while Im doing tutorial
            if (Globals.UsingTutorial)
                return;
            
            if (Instance == null)
                return;
            
            if (Instance.ShowingHint)
                return;

            if (objectsToHighlight.IsNullOrEmpty())
                Instance.ShowHint(hint);
            else 
                Instance.ShowHint(hint, objectsToHighlight);
        }

        //====================================================================================================================//
        //Ensures that we don't call multiple coroutines when they're not needed
        private static readonly List<HINT> WaitingHints = new List<HINT>();

        /// <summary>
        /// Show hint after a time delay. Optional params can specify which objects should be highlighted for the hint.
        /// </summary>
        /// <param name="hint"></param>
        /// <param name="delayTime"></param>
        /// <param name="objectsToHighlight"></param>
        public static void TryShowHint(HINT hint, float delayTime, params object[] objectsToHighlight)
        {
            if (!USE_HINTS)
                return;
            
            if (Instance == null)
                return;
            
            if (Instance.ShowingHint)
                return;
            
            if(!WaitingHints.Contains(hint))
                WaitingHints.Add(hint);
            else
                return;
            
            Instance.StartCoroutine(WaitCoroutine(delayTime, () =>
            {
                WaitingHints.Remove(hint);
                
                //If we have an empty list, assume we want to obtain in it other ways
                if(objectsToHighlight.IsNullOrEmpty()) Instance.ShowHint(hint);
                else Instance.ShowHint(hint, objectsToHighlight);
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

            object[] objectsToHighlight = null;
            
            switch (hint)
            {
                //----------------------------------------------------------------------------------------------------//
                case HINT.NONE:
                    highlightManager.SetActive(false);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.MAGNET:
                    return;
                    objectsToHighlight = GameUI.Instance.GetHintElements(hint);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.BONUS:
                    var bonusShape = FindObjectOfType<ObstacleManager>().ActiveBonusShapes.FirstOrDefault();

                    //Ensure that the bonus shape is not null when we want to highlight it
                    if (bonusShape is IRecycled recycled && recycled.IsRecycled)
                        return;

                    objectsToHighlight = new object[]
                    {
                        bonusShape
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                case HINT.GUN:
                    var gunPart = LevelManager.Instance
                        .BotInLevel
                        .AttachedBlocks
                        .FirstOrDefault(x => x is Part part && part.Type == PART_TYPE.GUN) as Part;
                    
                    objectsToHighlight = new object[]
                    {
                        gunPart
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                /*case HINT.FUEL:
                    objectsToHighlight = GameUI.Instance.GetHintElements(hint);
                    break;*/
                //----------------------------------------------------------------------------------------------------//
                /*case HINT.HOME:
                    var hasBits = PlayerDataManager.GetBlockDatas().Any(x => x.ClassType.Equals(nameof(Bit)));
                    if (!hasBits)
                        return;
                    
                    objectsToHighlight = FindObjectOfType<UniverseMap>().GetHintElements(hint);
                    break;*/
                //----------------------------------------------------------------------------------------------------//
                /*case HINT.CRAFT_PART:
                    var hasPart = PlayerDataManager.GetCurrentPartsInStorage()
                        .Any(x => x.ClassType.Equals(nameof(Part)));

                    if (!hasPart)
                        return;

                    objectsToHighlight = FindObjectOfType<StorageUI>().GetHintElements(hint);
                    break;*/
                //----------------------------------------------------------------------------------------------------//
                case HINT.DAMAGE:
                    /*objectsToHighlight = FindObjectOfType<DroneDesigner>().GetHintElements(hint);

                    var canAfford = FindObjectOfType<DroneDesignUI>().CanAffordRepair;
                    var textIndex = canAfford ? 0 : 1;
                    
                    StartCoroutine(HintCoroutine(hint, textIndex, objectsToHighlight.FirstOrDefault()));*/
                    return;
                //----------------------------------------------------------------------------------------------------//
                case HINT.HEALTH:
                    objectsToHighlight = FindObjectOfType<GameUI>().GetHintElements(hint);
                    break;
                //--------------------------------------------------------------------------------------------------------//
                    //These should just be ambient hints, with no explicit highlight
                case HINT.MAP:
                    objectsToHighlight = new object[]
                    {
                        new Bounds
                        {
                            center = Vector3.right * Globals.GridSizeX,
                            size = Vector3.zero
                        }
                    };
                    break;

                //--------------------------------------------------------------------------------------------------------//
                case HINT.PICK_PART:
                case HINT.STAR:
                    objectsToHighlight = new object[]
                    {
                        new Bounds
                        {
                            center = Vector3.zero,
                            size = Vector3.zero
                        }
                    };
                    break;

                //--------------------------------------------------------------------------------------------------------//
                case HINT.LAYOUT:
                case HINT.ENTER_WRECK:
                case HINT.PATCH_TREE:
                    objectsToHighlight = FindObjectOfType<PatchTreeUI>().GetHintElements(hint);
                    break;

                
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
                //----------------------------------------------------------------------------------------------------//
            }

            if (objectsToHighlight.IsNullOrEmpty())
                throw new NullReferenceException("No objects to highlight");

            StartCoroutine(HintPagesCoroutine(hint, objectsToHighlight));
        }

        private void ShowHint(HINT hint, params object[] objectsToHighlight)
        {
            if (hint != HINT.NONE && PlayerDataManager.GetHint(hint))
                return;
            
            StartCoroutine(HintPagesCoroutine(hint, objectsToHighlight));
        }

        //====================================================================================================================//

        /// <summary>
        /// Highlight each element of objectsToHighlight with the HintData Text elements for this hint.
        /// If there are more Text Elements than objects, all overflow will highlight the last object in the list.
        /// </summary>
        /// <param name="hint"></param>
        /// <param name="objectsToHighlight"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private IEnumerator HintPagesCoroutine(HINT hint, IReadOnlyList<object> objectsToHighlight)
        {
            var buttonPressed = false;
            
            //Menu Controls Input
            //--------------------------------------------------------------------------------------------------------//
            
            void OnSubmitPerformed(InputAction.CallbackContext ctx)
            {
                if (ctx.ReadValueAsButton() == false)
                    return;

                buttonPressed = true;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            var hintData = hintRemoteData.GetHintData(hint);
            
            Time.timeScale = 0f;
            OnShowingHintAction?.Invoke(true);
            ShowingHint = true;
            //disable controller traversal
            UISelectHandler.SendNavigationEvents = false;
            
            _previousInputActionGroup = InputManager.CurrentActionMap;
            InputManager.SwitchCurrentActionMap(ACTION_MAP.MENU);

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                buttonPressed = true;
            });
            
            //FIXME I probably want all of these functionalities to occur in the Input Manager
            Utilities.Inputs.Input.Actions.MenuControls.Submit.performed += OnSubmitPerformed;
            
            for (var i = 0; i < hintData.hintTexts.Count; i++)
            {
                var objectToHighlight = i >= objectsToHighlight.Count ? objectsToHighlight.Last() : objectsToHighlight[i];
                
                switch (objectToHighlight)
                {
                    case Vector3 vector3:
                        highlightManager.Highlight(GetPositionAsBounds(vector3));
                        break;
                    case Vector2 vector2:
                        highlightManager.Highlight(GetPositionAsBounds(vector2));
                        break;
                    case IHasBounds iHasBounds:
                        highlightManager.Highlight(iHasBounds);
                        break;
                    case Bounds bounds:
                        highlightManager.Highlight(bounds);
                        break;
                    case RectTransform rectTransform:
                        highlightManager.Highlight(rectTransform);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(objectToHighlight), objectToHighlight, "Type not supported by Highlight");
                }

                ShowHintText(hintData.hintTexts[i]);

                //TODO Need to also include waiting for button Press
                yield return new WaitUntil(() => buttonPressed);
                
                buttonPressed = false;

                yield return null;
            }

            PlayerDataManager.SetHint(hint, true);
            
            InputManager.SwitchCurrentActionMap(_previousInputActionGroup);
            _previousInputActionGroup = ACTION_MAP.NULL;
            

            OnShowingHintAction?.Invoke(false);
            Time.timeScale = 1f;
            ShowingHint = false;
            //allow for controller traversal
            UISelectHandler.SendNavigationEvents = true;
            highlightManager.SetActive(false);
            Utilities.Inputs.Input.Actions.MenuControls.Submit.performed -= OnSubmitPerformed;
        }

        /// <summary>
        /// Used if we want to highlight a specific element, with specific text
        /// </summary>
        /// <param name="hint"></param>
        /// <param name="textIndex"></param>
        /// <param name="objectToHighlight"></param>
        /// <param name="setHint"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private IEnumerator HintCoroutine(HINT hint, int textIndex, object objectToHighlight, bool setHint = true)
        {
            var text = hintRemoteData.GetHintData(hint).hintTexts[textIndex];
            
            Time.timeScale = 0f;
            ShowingHint = true;
            
            _previousInputActionGroup = InputManager.CurrentActionMap;
            InputManager.SwitchCurrentActionMap(ACTION_MAP.MENU);

            var buttonPressed = false;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                buttonPressed = true;
            });

            switch (objectToHighlight)
            {
                case IHasBounds iHasBounds:
                    highlightManager.Highlight(iHasBounds);
                    break;
                case Bounds bounds:
                    highlightManager.Highlight(bounds);
                    break;
                case RectTransform rectTransform:
                    highlightManager.Highlight(rectTransform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectToHighlight), objectToHighlight, "Type not supported by Highlight");
            }

            ShowHintText(text);

            //TODO Need to also include waiting for button Press
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || buttonPressed);

            if(setHint)
                PlayerDataManager.SetHint(hint, setHint);
            
            InputManager.SwitchCurrentActionMap(_previousInputActionGroup);
            _previousInputActionGroup = ACTION_MAP.NULL;
            
            Time.timeScale = 1f;
            ShowingHint = false;
            highlightManager.SetActive(false);

        }

        //====================================================================================================================//
        

        private void ShowHintText(HintRemoteDataScriptableObject.HintText hintText)
        {
            this.hintText.text = hintText.shortText;
            infoText.text = hintText.longDescription;
        }

        private Bounds GetPositionAsBounds(in Vector2 worldPosition)
        {
            return new Bounds
            {
                center = worldPosition,
                size = Vector2.one * Constants.gridCellSize
            };
        }

        //Editor Functions
        //====================================================================================================================//

#if UNITY_EDITOR

        [Button]
        private void TestGunHighlight()
        {
            TryShowHint(HINT.GUN);
        }

        [Button]
        private void ClearHints()
        {
            foreach (HINT hint in Enum.GetValues(typeof(HINT)))
            {
                PlayerDataManager.SetHint(hint, false);
            }
        }
        
#endif
        
    }
    
    public interface IHasHintElement
    {
        object[] GetHintElements(HINT hint);
    }
}
