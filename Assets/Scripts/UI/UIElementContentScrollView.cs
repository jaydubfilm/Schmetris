
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.UI
{
    [Serializable]
    public class UIElementContentScrollView<T>
    {
        [SerializeField, Required]
        private RectTransform contentTransform;

        [SerializeField, Required]
        private GameObject contentPrefab;

        public void CreateList(IEnumerable<UIElement<T>> elements)
        {
            foreach (var element in elements)
            {
                element.transform.SetParent(contentTransform, false);
                element.transform.localScale = Vector3.one;
            }
        }


    }
}


