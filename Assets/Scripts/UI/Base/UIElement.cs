using System;
using Recycling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.UI
{
    public abstract class UIElement<T> : MonoBehaviour, IRecycled, ICustomRecycle where T: IEquatable<T>
    {
        protected UIElementContentScrollViewBase contentScrollView;
        
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

        public void SetContainer(in UIElementContentScrollViewBase container)
        {
            contentScrollView = container;
        }

        //============================================================================================================//


        public bool IsRecycled { get; set; }

        public virtual void CustomRecycle(params object[] args)
        {
            
        }
    }
}


