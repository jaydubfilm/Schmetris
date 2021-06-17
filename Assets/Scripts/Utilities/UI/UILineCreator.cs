using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    public static class UILineCreator
    {
        public static Image DrawConnection(in Transform container, in RectTransform connectionStart, in RectTransform connectionEnd, in Color color)
        {
            var startPosition = connectionStart.position;
            var endPosition = connectionEnd.position;

            var newLineImage = new GameObject().AddComponent<Image>();
            //newLineImage.name = $"Line_[{connectionStart}][{connectionEnd}]";
            newLineImage.color = color;


            var newLineTransform = (RectTransform) newLineImage.transform;

            newLineTransform.SetParent(container);
            newLineTransform.SetAsFirstSibling();

            newLineTransform.position = (startPosition + endPosition) / 2;

            newLineTransform.sizeDelta = new Vector2(Vector2.Distance(startPosition, endPosition), 5);

            newLineTransform.right = (startPosition - endPosition).normalized;

            return newLineImage;
        }
        public static Image DrawConnection(in Transform container, in RectTransform connectionStart, in RectTransform connectionEnd, in Image imagePrefab, in Color color)
        {
            var startPosition = connectionStart.position;
            var endPosition = connectionEnd.position;

            var newLineImage = Object.Instantiate(imagePrefab);
            newLineImage.color = color;


            var newLineTransform = (RectTransform) newLineImage.transform;

            newLineTransform.SetParent(container);
            newLineTransform.SetAsFirstSibling();

            newLineTransform.position = (startPosition + endPosition) / 2;

            newLineTransform.sizeDelta = new Vector2(Vector2.Distance(startPosition, endPosition), 5);

            newLineTransform.right = (startPosition - endPosition).normalized;

            return newLineImage;
        }
    }
}
