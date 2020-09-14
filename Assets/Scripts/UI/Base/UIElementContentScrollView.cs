
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
    public class UIElementContentScrollView<U, T> where U: UIElement<T> where T : IEquatable<T>
    {
        [SerializeField, Required]
        private RectTransform contentTransform;

        [SerializeField, Required]
        private GameObject contentPrefab;

        public List<U> Elements { get; private set; }

        //TODO: find a better method then the compareNames for the current cases using this comparison (botshapeeditorui)
        public U AddElement(T data, string gameObjectName = "", bool compareNames = false, bool allowDuplicate = false)
        {
            if (Elements == null)
                Elements = new List<U>();

            if (!allowDuplicate)
            {
                U exists;
                if (compareNames)
                    exists = FindElement(data, gameObjectName);
                else
                    exists = FindElement(data);

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

        public U FindElement(T data)
        {
            return Elements?.FirstOrDefault(x => x.data.Equals(data));
        }

        public U FindElement(T data, string name)
        {
            return Elements?.FirstOrDefault(x => x.data.Equals(data) && x.name == name);
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

            RemoveElementAtIndex(index);
        }

        public void RemoveElementAtIndex(int index)
        {
            Recycler.Recycle<U>(Elements[index]);
            Elements.RemoveAt(index);
        }

        public void ClearElements()
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
                Elements = new List<U>();

            foreach (var uiElement in Elements)
            {
                uiElement.gameObject.SetActive(state);
            }
        }

    }
}
