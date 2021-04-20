using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class RectTransformExtensions
    {
        /*public static void ClampToScreenSpace(this RectTransform rectTransform, in Canvas canvas, in Vector4 spacing)
        {
            RectTransformUtility.WorldToScreenPoint(null,
                (Vector2) rectTransform.position + Vector2.right * rectTransform.sizeDelta.x);

            RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, rectTransform);
        }*/
        public static void TryFitInScreenBounds(this RectTransform rectTransform, in Canvas canvas, in Vector2 spacing)
        {
            rectTransform.TryFitInScreenBounds(canvas.transform as RectTransform, spacing);
        }
        public static void TryFitInScreenBounds(this RectTransform rectTransform, in RectTransform canvasRectTransform,
            in float spacing)
        {
            rectTransform.TryFitInScreenBounds(canvasRectTransform, new Vector4(spacing,spacing,spacing,spacing));
        }
        public static void TryFitInScreenBounds(this RectTransform rectTransform, in RectTransform canvasRectTransform,
            in Vector2 spacing)
        {
            rectTransform.TryFitInScreenBounds(canvasRectTransform, new Vector4(spacing.x,spacing.x,spacing.y,spacing.y));
        }
        public static void TryFitInScreenBounds(this RectTransform rectTransform, in RectTransform canvasRectTransform, in Vector4 spacing)
        {
            var canvasSize = canvasRectTransform.sizeDelta;
            
            var pos = rectTransform.localPosition;

            var size = rectTransform.sizeDelta;
            var pivot = rectTransform.pivot;

            var sizes = new
            {
                left = size.x * pivot.x,
                right = size.x * (1f - pivot.x),
                up = size.y * pivot.y,
                down = size.y * (1f - pivot.y)
            };
            
            var screenBounds = new Vector2(canvasSize.x, canvasSize.y) / 2f;

            var delta = Vector3.zero;

            if (pos.x - sizes.left < -screenBounds.x)
            {
                delta.x = Mathf.Abs(pos.x - sizes.left) - screenBounds.x;
                delta.x += spacing.x;
            }
            else if (pos.x + sizes.right > screenBounds.x)
            {
                delta.x = -((pos.x + sizes.right) - screenBounds.x);
                delta.x -= spacing.y;
            }

            if (pos.y - sizes.down < -screenBounds.y)
            {
                delta.y = Mathf.Abs(pos.y - sizes.down) - screenBounds.y;
                delta.y += spacing.w;
            }
            else if (pos.y + sizes.up > screenBounds.y)
            {
                delta.y = -((pos.y + sizes.up) - screenBounds.y);
                delta.y -= spacing.z;
            }

            rectTransform.localPosition = pos + delta;
        }
    }
}
