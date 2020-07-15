using System;
using StarSalvager.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    [RequireComponent(typeof(Toggle))]
    public abstract class ToggleUIElement<T> : UIElement<T> where T : IEquatable<T>
    {
        protected Toggle Toggle
        {
            get
            {
                if (_toggle == null)
                    _toggle = GetComponent<Toggle>();

                return _toggle;
            }
        }

        private Toggle _toggle;

        public abstract void Init(T data, Action<T, bool> OnToggleChanged);

        public sealed override void Init(T data)
        {
            this.data = data;
        }
    }
}

