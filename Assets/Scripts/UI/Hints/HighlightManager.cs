using System;
using Recycling;
using Sirenix.OdinInspector;
using Spine.Unity;
using StarSalvager.Cameras;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Interfaces;
using UnityEngine;

namespace StarSalvager.UI.Hints
{
    public enum CORNER
    {
        TL,
        TR,
        BR,
        BL
    }
    
    public class HighlightManager : MonoBehaviour
    {
        [BoxGroup("Testing")] public RectTransform TEST_target;
        [BoxGroup("Testing")] public Vector2 TEST_multiplier = Vector2.one;

        [Button("One to One"), BoxGroup("Testing")]
        private void TEST()
        {
            SetActive(true);
            HighlightRect(TEST_target);
            
        }

        [Button("Size Multiplier"), BoxGroup("Testing")]
        private void TEST2()
        {
            SetActive(true);
            HighlightRect(TEST_target, TEST_multiplier);
        }

        [Button("Bot Highlight"), BoxGroup("Testing"), DisableInEditorMode]
        private void TEST3()
        {
            
            var core = (Part) LevelManager.Instance.BotObject.attachedBlocks[0];
            var partBounds = new Bounds
            {
                center = core.transform.position,
                size = Vector3.one
            };

            SetActive(true);
            HighlightWorldBounds(partBounds, TEST_multiplier);
        }


        //====================================================================================================================//
        //TODO Need to add ability to reflect this value to get opposite corners


        //Properties
        //====================================================================================================================//

        #region Properties

        [SerializeField, Required] private RectTransform textRectTransform;

        [SerializeField, Space(20f)] private Vector2 edgeSpacing;

        [SerializeField, Required] private RectTransform maskObjectRect;
        [SerializeField, Required] private RectTransform maskParentRect;
        
        [SerializeField, BoxGroup("Character")]
        private RectTransform anchorTransform;
        [SerializeField, BoxGroup("Character")]
        private SkeletonGraphic skeletonGraphic;
        [SerializeField, BoxGroup("Character")]
        private RectTransform textWindowRectTransform;


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

        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        private void Start()
        {
            SetActive(false);
        }

        //Highlight Functions
        //====================================================================================================================//

        public void Highlight(in RectTransform rectTransform)
        {
            SetActive(true);
            HighlightRect(rectTransform, TEST_multiplier);
        }

        public void Highlight(in IHasBounds iHasBounds)
        {
            Highlight(iHasBounds.GetBounds());
        }

        public void Highlight(in Bounds worldSpaceBounds)
        {
            SetActive(true);
            HighlightWorldBounds(worldSpaceBounds, TEST_multiplier);
        }

        public void Highlight(in Vector2 worldSpacePosition)
        {
            SetActive(true);
            HighlightWorldPoint(worldSpacePosition, TEST_multiplier);
        }

        //Highlight RectTransform
        //====================================================================================================================//

        #region Highlight RectTransform

        private void HighlightRect(in RectTransform target)
        {
            HighlightRect(target, Vector2.one);
        }

        private void HighlightRect(in RectTransform target, in Vector2 sizeMultiplier)
        {

           // titleText.text = text;
            var corner = RepositionText(target);

            var highlightRect = GetRectAround(target, textRectTransform);

            maskParentRect.localPosition = highlightRect.center;
            maskParentRect.sizeDelta = highlightRect.size * sizeMultiplier;


            TryFitInScreenBounds(CanvasRectTransform.sizeDelta, edgeSpacing, ref maskParentRect);
            
            TrySetCharacterPosition(RectToCanvasSpace(target), corner);

            ResetMask();
        }

        #endregion //Highlight RectTransform

        //Highlight World Bounds
        //====================================================================================================================//

        #region Highlight World Bounds

        private void HighlightWorldBounds(in Bounds worldSpaceBounds)
        {
            HighlightWorldBounds(worldSpaceBounds, Vector2.one);
        }

        private void HighlightWorldBounds(in Bounds worldSpaceBounds, in Vector2 sizeMultiplier)
        {
            var canvasSpaceBounds = WorldToCanvasSpaceBounds(CanvasRectTransform, worldSpaceBounds);

            var corner = RepositionText(canvasSpaceBounds);
            

            var highlightRect = GetRectAround(canvasSpaceBounds, textRectTransform);

            maskParentRect.localPosition = highlightRect.center;
            maskParentRect.sizeDelta = highlightRect.size * sizeMultiplier;

            TryFitInScreenBounds(CanvasRectTransform.sizeDelta, edgeSpacing, ref maskParentRect);

            TrySetCharacterPosition(new Rect
            {
                center = canvasSpaceBounds.center,
                size =  canvasSpaceBounds.size
            }, corner);
            
            ResetMask();
        }

        #endregion //Highlight World Bounds

        //Highlight World Position
        //====================================================================================================================//

        private void HighlightWorldPoint(Vector3 worldPoint, Vector2 size)
        {
            throw new NotImplementedException();
        }

        //====================================================================================================================//

        public void SetActive(bool state)
        {
            textRectTransform.gameObject.SetActive(state);
            
            maskParentRect.gameObject.SetActive(state);

            anchorTransform.gameObject.SetActive(state);
        }

        /// <summary>
        /// Tries to find a position for the character text box that does not overlap the text element, and the highlighted element.
        /// If all positions were blocked, default back to original
        /// </summary>
        /// <param name="targetRect"></param>
        /// <param name="corner"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void TrySetCharacterPosition(Rect targetRect, in CORNER corner)
        {
            var overlapCount = 0;
            var checkCorner = corner;
            while (true)
            {
                //Want to ensure that the character that is giving the relevant information is out of the way
                var reflectedCorner = checkCorner.Reflected();

                var skeletonRectTransform = (RectTransform) skeletonGraphic.transform;


                Vector2 windowAnchorPosition = textWindowRectTransform.anchoredPosition;
                Vector2 skeletonAnchorPosition = skeletonRectTransform.anchoredPosition;

                Vector3 skeletonLocalScale = skeletonGraphic.transform.localScale;

                switch (reflectedCorner)
                {
                    //Character on Left
                    //----------------------------------------------------------------------------------------------------//
                    case CORNER.BL:
                    case CORNER.TL:
                        anchorTransform.anchorMax = anchorTransform.anchorMin = new Vector2(0, 0.5f);

                        skeletonLocalScale.x = Mathf.Abs(skeletonLocalScale.x);

                        skeletonAnchorPosition.x = Mathf.Abs(skeletonAnchorPosition.x);
                        windowAnchorPosition.x = Mathf.Abs(windowAnchorPosition.x);

                        break;

                    //Character on Right
                    //----------------------------------------------------------------------------------------------------//
                    case CORNER.TR:
                    case CORNER.BR:
                        anchorTransform.anchorMax = anchorTransform.anchorMin = new Vector2(1, 0.5f);
                        skeletonGraphic.initialFlipX = true;

                        skeletonLocalScale.x = -Mathf.Abs(skeletonLocalScale.x);

                        skeletonAnchorPosition.x = -Mathf.Abs(skeletonAnchorPosition.x);
                        windowAnchorPosition.x = -Mathf.Abs(windowAnchorPosition.x);
                        break;
                    //----------------------------------------------------------------------------------------------------//
                    default:
                        throw new ArgumentOutOfRangeException(nameof(checkCorner), checkCorner, null);
                }

                switch (reflectedCorner)
                {
                    case CORNER.TL:
                    case CORNER.TR:
                        windowAnchorPosition.y = Mathf.Abs(textWindowRectTransform.sizeDelta.y);
                        break;
                    case CORNER.BR:
                    case CORNER.BL:
                        windowAnchorPosition.y = -Mathf.Abs(textWindowRectTransform.sizeDelta.y);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                textWindowRectTransform.anchoredPosition = windowAnchorPosition;

                skeletonRectTransform.anchoredPosition = skeletonAnchorPosition;
                skeletonRectTransform.transform.localScale = skeletonLocalScale;

                if (overlapCount >= 4)
                    break;

                var compareRect = RectToCanvasSpace(textWindowRectTransform);
                var textRect = RectToCanvasSpace(textRectTransform);
                
                if (compareRect.Overlaps(targetRect) || compareRect.Overlaps(textRect))
                {
                    Debug.LogError("Overlap happening");
                    
                    SSDebug.DrawSquare(
                        CanvasRectTransform.TransformPoint(compareRect.min),
                        CanvasRectTransform.TransformPoint(compareRect.max), 
                        Color.red,
                        5f);

                    overlapCount++;
                    checkCorner = checkCorner.Next();
                    continue;
                }

                break;
            }
        }

        //Get Data RectTransform
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
                CanvasRectTransform.TransformPoint(rect1.max), Color.yellow, 5f);
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(rect2.min),
                CanvasRectTransform.TransformPoint(rect2.max), Color.blue, 5f);
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
        private CORNER RepositionText(in RectTransform rectTransform)
        {
            var quadrant = FindQuadrant(rectTransform);
            var reflectedCorner = quadrant.Reflected();

            textRectTransform.position = rectTransform.position;

            //Get the opposite corner of the rect, to space starting from there
            var positionDelta = GetDistanceToCorner(reflectedCorner, rectTransform);
            positionDelta += GetDistanceToCorner(reflectedCorner, textRectTransform) *
                             new Vector2(1f, 1.9f);

            textRectTransform.localPosition += (Vector3) positionDelta;

            return quadrant;
        }

        //Get Data Bounds
        //====================================================================================================================//

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
                CanvasRectTransform.TransformPoint(canvasSpaceBounds.max), Color.yellow, 5f);
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(rect.min),
                CanvasRectTransform.TransformPoint(rect.max), Color.blue, 5f);
            SSDebug.DrawSquare(CanvasRectTransform.TransformPoint(outRect.min),
                CanvasRectTransform.TransformPoint(outRect.max), Color.green, 5f);

            return outRect;
        }

        /// <summary>
        /// Reposition the text element to be offset of the target RectTransform relative to its position on screen
        /// </summary>
        /// <param name="canvasSpaceBounds"></param>
        private CORNER RepositionText(in Bounds canvasSpaceBounds)
        {
            //var titleTransform = textRectTransform;
            var quadrant = FindQuadrant(canvasSpaceBounds.center);
            var reflectedCorner = quadrant.Reflected();


            var localPosition = canvasSpaceBounds.center;

            //Get the opposite corner of the rect, to space starting from there
            var positionDelta = GetDistanceToCorner(reflectedCorner, canvasSpaceBounds);
            positionDelta += GetDistanceToCorner(reflectedCorner, textRectTransform) *
                             new Vector2(1f, 1.9f);

            localPosition += (Vector3) positionDelta;
            textRectTransform.localPosition = localPosition;

            return quadrant;
        }

        //HintManager Functions
        //====================================================================================================================//

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

        private static CORNER FindQuadrant(in Vector2 canvasPoint)
        {
            var localPosition = canvasPoint;

            if (localPosition.x < 0 && localPosition.y >= 0)
                return CORNER.TL;

            if (localPosition.x > 0 && localPosition.y >= 0)
                return CORNER.TR;

            if (localPosition.x > 0 && localPosition.y < 0)
                return CORNER.BR;

            return CORNER.BL;
        }

        private static Vector3 ToCanvasSpacePosition(in Canvas canvas, in Vector3 worldPosition)
        {
            return ((RectTransform) canvas.transform).InverseTransformPoint(worldPosition);
        }

        private static void TryFitInScreenBounds(in Vector2 canvasSize, in Vector2 spacing,
            ref RectTransform rectTransform)
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

        private static Bounds WorldToCanvasSpaceBounds(in RectTransform canvasTransform, in Bounds worldSpaceBounds)
        {
            var minScreenPoint = CameraController.Camera.WorldToScreenPoint(worldSpaceBounds.min);
            var maxScreenPoint = CameraController.Camera.WorldToScreenPoint(worldSpaceBounds.max);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, minScreenPoint, null,
                out var canvasMinPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, maxScreenPoint, null,
                out var canvasMaxPoint);

            var canvasSpaceBounds = new Bounds
            {
                min = canvasMinPoint,
                max = canvasMaxPoint
            };

            return canvasSpaceBounds;
        }

        //====================================================================================================================//

    }

    public static class CornerExtensions
    {
        public static CORNER Next(this CORNER corner)
        {
            switch (corner)
            {
                case CORNER.TL:
                    return CORNER.TR;
                case CORNER.TR:
                    return CORNER.BR;
                case CORNER.BR:
                    return CORNER.BL;
                case CORNER.BL:
                    return CORNER.TL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }
        //TODO Can add functions for flipping horizontally/vertically
        public static CORNER Reflected(this CORNER corner)
        {
            switch (corner)
            {
                case CORNER.TL:
                    return CORNER.BR;
                case CORNER.TR:
                    return CORNER.BL;
                case CORNER.BR:
                    return CORNER.TL;
                case CORNER.BL:
                    return CORNER.TR;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }
        
        public static CORNER VerticalMirror(this CORNER corner)
        {
            switch (corner)
            {
                case CORNER.TL:
                    return CORNER.BL;
                case CORNER.TR:
                    return CORNER.BR;
                case CORNER.BR:
                    return CORNER.TR;
                case CORNER.BL:
                    return CORNER.TL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }
        
        public static CORNER HorizontalMirror(this CORNER corner)
        {
            switch (corner)
            {
                case CORNER.TL:
                    return CORNER.TR;
                case CORNER.TR:
                    return CORNER.TL;
                case CORNER.BR:
                    return CORNER.BL;
                case CORNER.BL:
                    return CORNER.BR;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }

    }
}
