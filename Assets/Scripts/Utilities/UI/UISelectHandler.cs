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
    public struct NavigationException
    {
        [Flags]
        public enum DIRECTION: int
        {
            LEFT = 0,
            UP = 1 << 0,
            RIGHT = 1 << 1,
            DOWN = 1 << 2
        }
            
        public DIRECTION Direction;
        public Selectable Selectable;
            
        public bool ContainsMode(in DIRECTION direction)
        {
            return Direction.HasFlag(direction);
        }
    }
    
    [RequireComponent(typeof(EventSystem))]
    public class UISelectHandler : Singleton<UISelectHandler>, IStartedUsingController
    {
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

        //====================================================================================================================//
        public static void SetupNavigation(in Selectable objectToSelect, in IEnumerable<Selectable> selectables, in IEnumerable<NavigationException> exceptions = null)
        {
            Instance.SetupNavigation(selectables.ToArray(), exceptions?.ToArray());
            Instance._startingSelectable = Instance._currentSelectable = objectToSelect;
        }

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

        

        //Unity Functions
        //====================================================================================================================//

        private void OnEnable()
        {
            InputManager.AddStartedControllerListener(this);
            StartedUsingController(false);
        }

        private void OnDisable()
        {
            InputManager.RemoveControllerListener(this);
        }

        //====================================================================================================================//
        private Selectable[] _currentSelectables;

        private void SetupNavigation(Selectable[] selectables, NavigationException[] exceptions)
        {
            IEnumerator WaitForFinish()
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                
                selectables.FillNavigationOptions(exceptions);
                
                if (Instance._usingController) TrySelectObject(_currentSelectable);
            }
            
            //_currentSelectables?.CleanNavigationOptions();

            //Want to copy the data 
            _currentSelectables = new List<Selectable>(selectables).ToArray();

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
            _outlineTransform.SetSiblingIndex(siblingIndex);

            _outlineTransform.position = rectTransform.position;
            _outlineTransform.sizeDelta = rectTransform.sizeDelta * sizeMultiplier;
            _outline.color = color;
        }

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
            StartCoroutine(ActivateNavigation());
        }

        //====================================================================================================================//
        
        private void SetActive(in bool state)
        {
            if (_outline is null) return;

            _outline.gameObject.SetActive(state);
        }

        //====================================================================================================================//

    }
}
