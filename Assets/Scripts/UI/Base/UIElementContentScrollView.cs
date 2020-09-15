﻿
using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
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

        //TODO: find a better method then the compareNames for the current cases using this comparison (botshapeeditorui)
        public U AddElement<U>(T data, string gameObjectName = "", bool compareNames = false, bool allowDuplicate = false) where U: UIElement<T>
        {
            if (Elements == null)
                Elements = new List<UIElement<T>>();

            if (!allowDuplicate)
            {
                U exists;
                if (compareNames)
                    exists = FindElement<U>(data, gameObjectName);
                else
                    exists = FindElement<U>(data);

                if (exists != null)
                    return exists;
            }

            var element = Object.Instantiate(contentPrefab).GetComponent<U>();

            if (!string.IsNullOrEmpty(gameObjectName))
                element.gameObject.name = gameObjectName;

            element.transform.SetParent(contentTransform, false);
            element.transform.localScale = Vector3.one;

            Elements.Add(element);

            return element;
        }

        public U FindElement<U>(T data) where U : UIElement<T>
        {
            return (U) Elements?.FirstOrDefault(x => x.data.Equals(data));
        }

        public U FindElement<U>(T data, string name) where U : UIElement<T>
        {
            return (U)Elements?.FirstOrDefault(x => x.data.Equals(data) && x.name == name);
        }

        public void RemoveElement<U>(T data) where U : UIElement<T>
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

            RemoveElementAtIndex<U>(index);
        }

        public void RemoveElementAtIndex<U>(int index) where U : UIElement<T>
        {
            Recycler.Recycle<U>(Elements[index]);
            Elements.RemoveAt(index);
        }

        public void ClearElements<U>() where U : UIElement<T> 
        {
            if (Elements == null)
                return;

            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                Recycler.Recycle<U>(Elements[i]);
                Elements.RemoveAt(i);
            }
        }

        /// <summary>
        /// Sets the UIElements gameObject.SetActive to the state provided
        /// </summary>
        /// <param name="state"></param>
        public void SetElementsActive(bool state)
        {
            if (Elements == null)
                Elements = new List<UIElement<T>>();

            foreach (var uiElement in Elements)
            {
                uiElement.gameObject.SetActive(state);
            }
        }
    }
}
