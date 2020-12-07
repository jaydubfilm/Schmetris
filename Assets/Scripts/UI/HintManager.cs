using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
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
        
        [Button("Bot Highlight"),BoxGroup("Testing"), DisableInEditorMode]
        private void TEST3()
        {
            var core = (Part)LevelManager.Instance.BotObject.attachedBlocks[0];
            var partBounds = new Bounds
            {
                center = core.transform.position,
                size = Vector3.one
            };
            
            
            HighlightWorldBounds(core.gameObject.name, partBounds);
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


        private RectTransform CanvasRectTransform
        {
            get
            {
                if (_canvasRectTransform != null)
                    return _canvasRectTransform;

                if (Canvas != null && Canvas.transform is RectTransform rectTransform)
                    _canvasRectTransform = rectTransform;

                return _canvasRectTransform;
            }
        }
        private RectTransform _canvasRectTransform;

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
            
            maskParentRect.localPosition = highlightRect.center;
            maskParentRect.sizeDelta = highlightRect.size * sizeMultiplier;

            
            TryFitInScreenBounds(CanvasRectTransform.sizeDelta, edgeSpacing, ref maskParentRect);
            

            ResetMask();
        }

        private void HighlightWorldBounds(in string text, in Bounds worldSpaceBounds)
        {

            var minScreenPoint = CameraController.Camera.WorldToScreenPoint(worldSpaceBounds.min);
            var maxScreenPoint = CameraController.Camera.WorldToScreenPoint(worldSpaceBounds.max);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRectTransform, minScreenPoint, null,
                out var canvasMinPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRectTransform, maxScreenPoint, null,
                out var canvasMaxPoint);
            
            var canvasSpaceBounds = new Bounds
            {
                min = canvasMinPoint,
                max = canvasMaxPoint
            };
            
            titleText.text = text;
            RepositionText(canvasSpaceBounds);
            
            var highlightRect = GetRectAround(canvasSpaceBounds, titleText.transform as RectTransform);
            
            maskParentRect.localPosition = highlightRect.center;
            maskParentRect.sizeDelta = highlightRect.size;

            
            TryFitInScreenBounds(CanvasRectTransform.sizeDelta, edgeSpacing, ref maskParentRect);
            

            ResetMask();
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

        /// <summary>
        /// Returns the rect in canvas space coordinates surrounding the Min & Max of both rects.
        /// </summary>
        /// <param name="rectTransform1"></param>
        /// <param name="rectTransform2"></param>
        /// <returns></returns>
        private Rect GetRectAround(RectTransform rectTransform1, RectTransform rectTransform2)
        {

            var rect1 = RectToCanvasSpace(rectTransform1);
            var rect2 = RectToCanvasSpace(rectTransform2);

            var maxX = Mathf.Max(rect1.xMax, rect2.xMax);
            var minX = Mathf.Min(rect1.xMin, rect2.xMin);

            var maxY = Mathf.Max(rect1.yMax, rect2.yMax);
            var minY = Mathf.Min(rect1.yMin, rect2.yMin);

            var outRect = new Rect
            {
                max = new Vector2(maxX, maxY),
                min = new Vector2(minX, minY)
            };
            
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(rect1.min),
                CanvasRectTransform.TransformPoint(rect1.max), Color.red, 5f);
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(rect2.min),
                CanvasRectTransform.TransformPoint(rect2.max), Color.blue, 5f);
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(outRect.min),
                CanvasRectTransform.TransformPoint(outRect.max), Color.green, 5f);

            return outRect;
        }

        /// <summary>
        /// Returns the rect in canvas space coordinates surrounding the Min & Max of both rects.
        /// </summary>
        /// <param name="canvasSpaceBounds"></param>
        /// <param name="rectTransform1"></param>
        /// <returns></returns>
        private Rect GetRectAround(Bounds canvasSpaceBounds, RectTransform rectTransform1)
        {

            var rect = RectToCanvasSpace(rectTransform1);

            var maxX = Mathf.Max(canvasSpaceBounds.max.x, rect.xMax);
            var minX = Mathf.Min(canvasSpaceBounds.min.x, rect.xMin);

            var maxY = Mathf.Max(canvasSpaceBounds.max.y, rect.yMax);
            var minY = Mathf.Min(canvasSpaceBounds.min.y, rect.yMin);

            var outRect = new Rect
            {
                max = new Vector2(maxX, maxY),
                min = new Vector2(minX, minY)
            };
            
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(canvasSpaceBounds.min),
                CanvasRectTransform.TransformPoint(canvasSpaceBounds.max), Color.red, 5f);
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(rect.min),
                CanvasRectTransform.TransformPoint(rect.max), Color.blue, 5f);
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(outRect.min),
                CanvasRectTransform.TransformPoint(outRect.max), Color.green, 5f);

            return outRect;
        }

        /// <summary>
        /// Converts the RectTransform into canvas space
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        private Rect RectToCanvasSpace(in RectTransform rectTransform)
        {
            var rectTransformBounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(CanvasRectTransform, rectTransform);

            return new Rect
            {
                center = rectTransformBounds.center,
                min = rectTransformBounds.min,
                max = rectTransformBounds.max
            };
        }

        /// <summary>
        /// Reposition the text element to be offset of the target RectTransform relative to its position on screen
        /// </summary>
        /// <param name="rectTransform"></param>
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
        
        /// <summary>
        /// Reposition the text element to be offset of the target RectTransform relative to its position on screen
        /// </summary>
        /// <param name="bounds"></param>
        private void RepositionText(in Bounds bounds)
        {
            var quadrant = FindQuadrant(Canvas, bounds);
            var reflectedCorner = quadrant.Reflected();
            
            titleText.transform.position = bounds.center;

            /*
            //Get the opposite corner of the rect, to space starting from there
            var positionDelta = GetDistanceToCorner(reflectedCorner, bounds);
            positionDelta += GetDistanceToCorner(reflectedCorner, titleText.transform as RectTransform) *
                             new Vector2(1f, 1.9f);

            titleText.transform.localPosition += (Vector3)positionDelta;*/
        }
        
        private void ResetMask()
        {
            var canvasSize = CanvasRectTransform.sizeDelta;
            
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
        private static Vector2 GetDistanceToCorner(CORNER corner, in Bounds bounds)
        {
            var rect = bounds.extents;
            
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
        
        private static CORNER FindQuadrant(Canvas canvas, in Bounds bounds)
        {
            var localPosition = ToCanvasSpacePosition(canvas, bounds.center);

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
