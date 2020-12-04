using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Debugging;
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
            HighlightRect(TEST_target.name, TEST_target);
        }
        
        [Button("Size Multiplier"),BoxGroup("Testing")]
        private void TEST2()
        {
            HighlightRect(TEST_target.name, TEST_target, TEST_multiplier);
        }


        //====================================================================================================================//
        //TODO Need to add ability to reflect this value to get opposite corners
        public enum CORNER
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

            titleText.text = text;
            RepositionText(target);

            var highlightRect = GetRectAround(target, titleText.transform as RectTransform);
            
            var canvasRectTransform = (RectTransform)Canvas.transform;
            //var targetRect = target.rect;

            // maskParentRect.pivot = target.pivot;
            maskParentRect.localPosition = highlightRect.center;
            maskParentRect.sizeDelta = highlightRect.size * sizeMultiplier;

            //TryFitInScreenBounds(canvasRectTransform.sizeDelta, edgeSpacing, ref maskParentRect);
            
            Debug.Log($"Center: {highlightRect.center}");



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

        //====================================================================================================================//

        private Rect GetRectAround(RectTransform rectTransform1, RectTransform rectTransform2)
        {
            var canvasRectTransform = (RectTransform) Canvas.transform;

            var rect1 = RectToCanvasSpace(rectTransform1);
            var rect2 = RectToCanvasSpace(rectTransform2);

            var maxX = Mathf.Max(rect1.xMax, rect2.xMax);
            var minX = Mathf.Min(rect1.xMin, rect2.xMin);

            var maxY = Mathf.Max(rect1.yMax, rect2.yMax);
            var minY = Mathf.Min(rect1.yMin, rect2.yMin);

            var size = new Vector2(maxX, maxY) - new Vector2(minX, minY);


            var outRect = new Rect
            {
                center = Vector2.Lerp(rect1.center, rect2.center, 0.5f),
                size = size
                /*max = new Vector2(maxX, maxY),
                min = new Vector2(minX, minY),*/
            };

            SSDebug.DrawSquare(canvasRectTransform.TransformPoint(rect1.min),
                canvasRectTransform.TransformPoint(rect1.max), Color.red, 5f);
            SSDebug.DrawSquare(canvasRectTransform.TransformPoint(rect2.min),
                canvasRectTransform.TransformPoint(rect2.max), Color.blue, 5f);
            SSDebug.DrawSquare(canvasRectTransform.TransformPoint(outRect.min),
                canvasRectTransform.TransformPoint(outRect.max), Color.green, 5f);

            return outRect;
        }

        private Rect RectToCanvasSpace(RectTransform rectTransform)
        {
            var canvasRectTransform = (RectTransform) Canvas.transform;

            var rect = rectTransform.rect;
            
            var size = new Vector2(rect.width, rect.height);
            var center = (Vector2)canvasRectTransform.InverseTransformPoint(rectTransform.position);
            /*var max = center + new Vector2(size.x / 2f, size.y / 2f);
            var min = center - new Vector2(size.x / 2f, size.y / 2f);*/
            
            //Debug.Log($"{rectTransform.name} Center: {center} Size: {size}", rectTransform);
            
            return new Rect
            {
                center = center,
                size = size
                /*max = max,
                min = min*/
            };

        }

        private void RepositionText(in RectTransform rectTransform)
        {
            var quadrant = FindQuadrant(rectTransform);
            var reflectedCorner = quadrant.Reflected();
            
            titleText.transform.position = rectTransform.position;

            //Get the opposite corner of the rect, to space starting from there
            var positionDelta = GetDistanceToCorner(reflectedCorner, rectTransform);
            positionDelta += GetDistanceToCorner(reflectedCorner, titleText.transform as RectTransform) *
                             new Vector2(1f, 1.9f);

            titleText.transform.localPosition += (Vector3)positionDelta;
        }
        
        private void ResetMask()
        {
            var canvasSize = ((RectTransform)Canvas.transform).sizeDelta;
            
            maskObjectRect.position = maskParentRect.parent.TransformPoint(Vector3.zero);
            maskObjectRect.sizeDelta = new Vector2(canvasSize.x, canvasSize.y);
        }
        
        //====================================================================================================================//
        //FIXME This will need to account for pivots that are not in the center
        //FIXME Use corner reflection to get correct values
        private static Vector2 GetDistanceToCorner(CORNER corner, RectTransform rectTransform)
        {
            var rect = rectTransform.rect.size / 2f;
            
            //TODO Reflect corner

            switch (corner)
            {
                case CORNER.TL:
                    return new Vector2(-rect.x, rect.y);
                case CORNER.TR:
                    return new Vector2(rect.x, rect.y);
                case CORNER.BR:
                    return new Vector2(rect.x, -rect.y);
                case CORNER.BL:
                    return new Vector2(-rect.x, -rect.y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }
        
        private static CORNER FindQuadrant(in RectTransform target)
        {
            var canvas = target.GetComponentInParent<Canvas>();
            var localPosition = ToCanvasSpacePosition(canvas, target.position);

            if (localPosition.x < 0 && localPosition.y > 0)
                return CORNER.TL;

            if (localPosition.x > 0 && localPosition.y > 0)
                return CORNER.TR;

            if (localPosition.x > 0 && localPosition.y < 0)
                return CORNER.BR;

            return CORNER.BL;
        }

        private static Vector3 ToCanvasSpacePosition(in Canvas canvas, in Vector3 worldPosition)
        {
            return ((RectTransform) canvas.transform).InverseTransformPoint(worldPosition);
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

    public static class CornerExtensions
    {
        //TODO Can add functions for flipping horizontally/vertically
        public static HintManager.CORNER Reflected(this HintManager.CORNER corner)
        {
            switch (corner)
            {
                case HintManager.CORNER.TL:
                    return HintManager.CORNER.BR;
                case HintManager.CORNER.TR:
                    return HintManager.CORNER.BL;
                case HintManager.CORNER.BR:
                    return HintManager.CORNER.TL;
                case HintManager.CORNER.BL:
                    return HintManager.CORNER.TR;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }
    }
}
