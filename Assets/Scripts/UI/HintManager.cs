﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI
{
    public class HintManager : MonoBehaviour
    {
        [BoxGroup("Testing")]
        public RectTransform TEST_target;
        [BoxGroup("Testing")]
        public Vector2 TEST_multiplier = Vector2.one;
        
        [Button("One to One"), BoxGroup("Testing")]
        private void TEST()
        {
            HighlightRect("", TEST_target);
        }
        
        [Button("Size Multiplier"),BoxGroup("Testing")]
        private void TEST2()
        {
            HighlightRect("", TEST_target, TEST_multiplier);
        }


        //====================================================================================================================//
        private enum CORNER
        {
            TL,
            TR,
            BR,
            BL
        }

        [SerializeField]
        private TMP_Text titleText;
        [SerializeField]
        private TMP_Text descriptionText;
        

        [SerializeField, Space(20f)] 
        private Vector2 edgeSpacing;
        
        [SerializeField, Required]
        private RectTransform maskObjectRect;
        [SerializeField, Required]
        private RectTransform maskParentRect;

        private Camera _camera;

        private Canvas Canvas
        {
            get
            {
                if (_canvas != null)
                    return _canvas;

                _canvas = GetComponentInParent<Canvas>();
                return _canvas;
            }
        }
        private Canvas _canvas;

        //====================================================================================================================//
        
        private void HighlightRect(in string text, in RectTransform target)
        {
            HighlightRect(text, target, Vector2.one);
        }
        private void HighlightRect(in string text, in RectTransform target, in Vector2 sizeMultiplier)
        {
            var canvasRectTransform = (RectTransform)Canvas.transform;
            var targetRect = target.rect;

            maskParentRect.pivot = target.pivot;
            maskParentRect.position = target.position;
            maskParentRect.sizeDelta = new Vector2(targetRect.width, targetRect.height) * sizeMultiplier;

            TryFitInScreenBounds(canvasRectTransform.sizeDelta, edgeSpacing, ref maskParentRect);



            ResetMask();
        }

        private void HighlightScreenPoint(in string text, Vector2 screenPoint, Vector2 size)
        {
            throw new NotImplementedException();
        }
        private void HighlightWorldPoint(in string text, Vector3 worldPoint, Vector2 size)
        {
            throw new NotImplementedException();
        }




        //====================================================================================================================//

        public void SetActive(bool state)
        {
            maskParentRect.gameObject.SetActive(state);
        }
        
        private void ResetMask()
        {
            var canvasSize = ((RectTransform)Canvas.transform).sizeDelta;
            
            maskObjectRect.position = maskParentRect.parent.TransformPoint(Vector3.zero);
            maskObjectRect.sizeDelta = new Vector2(canvasSize.x, canvasSize.y);
        }
        
        //====================================================================================================================//
        
        private static CORNER FindQuadrant(in RectTransform target)
        {
            var localPosition = target.localPosition;

            if (localPosition.x < 0 && localPosition.y > 0)
                return CORNER.TL;

            if (localPosition.x > 0 && localPosition.y > 0)
                return CORNER.TR;

            if (localPosition.x > 0 && localPosition.y < 0)
                return CORNER.BR;

            return CORNER.BL;
        }

        private static void TryFitInScreenBounds(in Vector2 canvasSize, in Vector2 spacing, ref RectTransform rectTransform)
        {
            var pos = rectTransform.localPosition;
            var sizeHalf = rectTransform.sizeDelta / 2f;
            var screenBounds = new Vector2(canvasSize.x, canvasSize.y) / 2f;

            var delta = Vector3.zero;

            if (pos.x - sizeHalf.x < -screenBounds.x)
            {
                delta.x = Mathf.Abs(pos.x - sizeHalf.x) - screenBounds.x;
                delta.x += spacing.x;
            }
            else if (pos.x + sizeHalf.x > screenBounds.x)
            {
                delta.x = -((pos.x + sizeHalf.x) - screenBounds.x);
                delta.x -= spacing.x;
            }
            
            if (pos.y - sizeHalf.y < -screenBounds.y)
            {
                delta.y = Mathf.Abs(pos.y - sizeHalf.y) - screenBounds.y;
                delta.y += spacing.y;
            }
            else if (pos.y + sizeHalf.y > screenBounds.y)
            {
                delta.y = -((pos.y + sizeHalf.y) - screenBounds.y);
                delta.y -= spacing.y;
            }

            rectTransform.localPosition = pos + delta;
        }

        //====================================================================================================================//

    }
}
