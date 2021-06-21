using System;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    public class UISelectHandler : Singleton<UISelectHandler>, IStartedUsingController
    {
        [SerializeField]
        private Image outlinePrefab;

        private Image _outline;
        private RectTransform _outlineTransform;

        //====================================================================================================================//
        
        public static void OutlineObject(in RectTransform rectTransform)
        {
            //Instance.Outline(rectTransform, Vector2.zero, Color.black);
        }
        public static void OutlineObject(in RectTransform rectTransform, in Vector2 sizeMultiplier, in Color color)
        {
            //Instance.Outline(rectTransform, sizeMultiplier, color);
        }

        //Unity Functions
        //====================================================================================================================//

        private void OnEnable()
        {
            InputManager.OnStartedUsingController += StartedUsingController;
        }

        private void OnDisable()
        {
            InputManager.OnStartedUsingController -= StartedUsingController;
        }

        //====================================================================================================================//
        

        private void Outline(in RectTransform rectTransform, in Vector2 sizeMultiplier, in Color color)
        {
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
            //var siblingIndex = rectTransform.GetSiblingIndex();
            
            _outlineTransform.SetParent(rectTransform.parent, false);
            _outlineTransform.SetSiblingIndex(0);

            _outlineTransform.position = rectTransform.position;
            _outlineTransform.sizeDelta = rectTransform.sizeDelta * sizeMultiplier;
            _outline.color = color;
        }

        public void StartedUsingController(bool usingController)
        {
            if (usingController) return;
            
            SetActive(false);
        }
        
        private void SetActive(in bool state)
        {
            if (_outline is null) return;
            
            _outline.gameObject.SetActive(state);
        }

        //====================================================================================================================//

    }
}
