using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.UI
{
    public abstract class UIElement<T> : MonoBehaviour where T: IEquatable<T>
    {
        [ShowInInspector, ReadOnly]
        public T data { get; protected set; }

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


