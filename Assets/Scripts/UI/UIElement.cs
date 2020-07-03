using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.UI
{
    public abstract class UIElement<T> : MonoBehaviour
    {
        public T data;

        //============================================================================================================//
        
        public RectTransform transform
        {
            get
            {
                if(_transform == null)
                    _transform = gameObject.transform as RectTransform;

                return _transform;
            }
        }

        private RectTransform _transform;

        //============================================================================================================//

        public abstract void Init(T data);

        //============================================================================================================//
    }
}


