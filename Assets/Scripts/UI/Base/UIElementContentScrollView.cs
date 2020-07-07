﻿
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.UI
{
    [Serializable]
    public class UIElementContentScrollView<T> where T : IEquatable<T>
    {
        [SerializeField, Required]
        private RectTransform contentTransform;

        [SerializeField, Required]
        private GameObject contentPrefab;

        public List<UIElement<T>> Elements { get; private set; }

        public U AddElement<U>(T data, string name = "") where U: UIElement<T>
        {
            if (Elements == null)
                Elements = new List<UIElement<T>>();

            var exists = FindElement<U>(data);

            if (exists != null)
                return exists;

            var element = Object.Instantiate(contentPrefab).GetComponent<U>();

            if (!string.IsNullOrEmpty(name))
                element.gameObject.name = name;
            
            element.transform.SetParent(contentTransform, false);
            element.transform.localScale = Vector3.one;
            
            Elements.Add(element);

            return element;
        }

        public U FindElement<U>(T data) where U : UIElement<T>
        {
            return (U) Elements?.FirstOrDefault(x => x.data.Equals(data));
        }
        
        public void RemoveElement(T data)
        {
            if (Elements == null)
                return;

            var index = -1;

            for (var i = 0; i < Elements.Count; i++)
            {
                if (!Elements[i].data.Equals(data))
                    continue;

                index = i;
                break;
                
            }

            if (index < 0)
                return;
            
            Elements.RemoveAt(index);

        }


    }
}


