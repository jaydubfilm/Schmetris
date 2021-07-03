using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{

    public interface IBuildNavigationProfile
    {
        NavigationProfile BuildNavigationProfile();
    }

    //Structs
    //====================================================================================================================//
    
    /// <summary>
    /// Ignores navigation to Selectable from the specified directions
    /// </summary>
    public struct NavigationRestriction
    {
        [Flags]
        public enum DIRECTION: int
        {
            LEFT = 0,
            UP = 1 << 0,
            RIGHT = 1 << 1,
            DOWN = 1 << 2
        }
            
        public DIRECTION FromDirection;
        public Selectable Selectable;
            
        public bool ContainsMode(in DIRECTION direction)
        {
            return FromDirection.HasFlag(direction);
        }
    }
    
    public struct NavigationOverride
    {
        public Selectable FromSelectable;
        
        public Selectable UpTarget;
        public Selectable DownTarget;
        public Selectable LeftTarget;
        public Selectable RightTarget;

    }

    public readonly struct NavigationProfile
    {
        public readonly Selectable ObjectToSelect;
        public readonly IEnumerable<Selectable> Selectables;
        public readonly IEnumerable<NavigationOverride> Overrides;
        public readonly IEnumerable<NavigationRestriction> Exceptions;

        public NavigationProfile(in Selectable objectToSelect, in IEnumerable<Selectable> selectables,
            in IEnumerable<NavigationOverride> overrides, in IEnumerable<NavigationRestriction> exceptions)
        {
            ObjectToSelect = objectToSelect;
            Selectables = selectables;
            Overrides = overrides;
            Exceptions = exceptions;
        }
    }

    //====================================================================================================================//
    
    
    [RequireComponent(typeof(EventSystem))]
    public class UISelectHandler : Singleton<UISelectHandler>, IStartedUsingController
    {
        public static Selectable CurrentlySelected =>
            Instance?.CurrentEventSystem?.currentSelectedGameObject.GetComponent<Selectable>();

        //Properties
        //====================================================================================================================//

        #region Properties

        [SerializeField]
        private Image outlinePrefab;

        private Image _outline;
        private RectTransform _outlineTransform;

        private Selectable _startingSelectable;
        private Selectable _currentSelectable;
        private bool _usingController;

        private EventSystem CurrentEventSystem
        {
            get
            {
                if (_currentEventSystem == null)
                    _currentEventSystem = GetComponent<EventSystem>();

                return _currentEventSystem;
            }
        }
        private EventSystem _currentEventSystem;

        #endregion //Properties
        
        private Selectable[] _currentSelectables;

        private static IBuildNavigationProfile _activeBuildTarget;
        private static NavigationProfile _currentNavigationProfile;

        //====================================================================================================================//
        public static void SetBuildTarget(in IBuildNavigationProfile buildNavigationProfile, in bool buildNow = true)
        {
            _activeBuildTarget = buildNavigationProfile;

            if (buildNow == false)
                return;
            
            RebuildNavigationProfile();
        }

        public static void RebuildNavigationProfile()
        {
            if (_activeBuildTarget == null) return;
            
            _currentNavigationProfile = _activeBuildTarget.BuildNavigationProfile();
            
            Instance.SetupNavigation(_currentNavigationProfile);
        }

        //Object Outlines
        //====================================================================================================================//

        #region Object Outlines

        public static void TrySelectObject(in Selectable selectable)
        {
            Instance._currentSelectable = selectable;
            Instance.CurrentEventSystem.SetSelectedGameObject(selectable.gameObject);
        }
        
        public static void OutlineObject(in RectTransform rectTransform)
        {
            Instance.Outline(rectTransform, Vector2.zero, Color.black);
        }
        public static void OutlineObject(in RectTransform rectTransform, in Vector2 sizeMultiplier, in Color color)
        {
            Instance.Outline(rectTransform, sizeMultiplier, color);
        }

        #endregion //Object Outlines

        //Unity Functions
        //====================================================================================================================//

        #region Unity Functions

        private void OnEnable()
        {
            InputManager.AddStartedControllerListener(this);
            StartedUsingController(false);
        }

        private void OnDisable()
        {
            InputManager.RemoveControllerListener(this);
        }

        #endregion //Unity Functions

        //====================================================================================================================//
       
        private void SetupNavigation(NavigationProfile navigationProfile)
        {

            //--------------------------------------------------------------------------------------------------------//
            
            IEnumerator WaitForFinish()
            {
                //We allow the UI time to update in case layout changes are occuring when this is called
                for (var i = 0; i < 2; i++)
                {
                    yield return new WaitForEndOfFrame();
                }
                
                navigationProfile.Selectables.FillNavigationOptions(navigationProfile.Exceptions, navigationProfile.Overrides);

                if (Instance._usingController)
                {
                    //In the event that the options are now null, find the first available selectable in the list provided
                    if (_currentSelectable == null && _startingSelectable == null)
                        TrySelectObject(navigationProfile.Selectables.FirstOrDefault(x =>
                            x.interactable && x.gameObject.activeInHierarchy));
                    else
                        TrySelectObject(_currentSelectable ? _currentSelectable : _startingSelectable);
                }
            }

            //--------------------------------------------------------------------------------------------------------//
            
            
            _startingSelectable = _currentSelectable = navigationProfile.ObjectToSelect;

            StartCoroutine(WaitForFinish());
        }

        //====================================================================================================================//
        
        private void Outline(in RectTransform rectTransform, in Vector2 sizeMultiplier, in Color color)
        {
            if (!_usingController) return;
            
            if (_outline == null)
            {
                _outline = Instantiate(outlinePrefab);

                var layoutElement = _outline.gameObject.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                _outlineTransform = (RectTransform)_outline.transform;
            }

            if (rectTransform == null)
            {
                SetActive(false);
                return;
            }

            _outlineTransform.SetParent(null);

            SetActive(true);
            var siblingIndex = rectTransform.GetSiblingIndex();

            _outlineTransform.SetParent(rectTransform.parent, false);
            _outlineTransform.localScale = Vector3.one;
            _outlineTransform.SetSiblingIndex(siblingIndex);

            _outlineTransform.position = rectTransform.position;
            _outlineTransform.sizeDelta = rectTransform.sizeDelta * sizeMultiplier;
            _outline.color = color;
        }

        //IStartedUsingController Functions
        //====================================================================================================================//
        
        public void StartedUsingController(bool usingController)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            IEnumerator ActivateNavigation()
            {
                yield return new WaitForSeconds(0.2f);
                CurrentEventSystem.sendNavigationEvents = true;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            _usingController = usingController;

            if (!_usingController)
            {
                CurrentEventSystem.sendNavigationEvents = false;
                SetActive(false);
                return;
            }

            TrySelectObject(_startingSelectable);
            SetActive(true);
            StartCoroutine(ActivateNavigation());
        }

        //====================================================================================================================//
        
        private void SetActive(in bool state)
        {
            _outline?.gameObject.SetActive(state);
        }

        //====================================================================================================================//

    }
}
