using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    //[RequireComponent(typeof(Button))]
    public abstract class ButtonUIElement<T> : UIElement<T> where T : IEquatable<T>
    {
        protected Button button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();

                return _button;
            }
        }

        [SerializeField, Required]
        private Button _button;

        public abstract void Init(T data, Action OnPressed);

        public sealed override void Init(T data)
        {
            this.data = data;
        }

        /*protected void ForceSetButton(in Button button)
        {
            _button = button;
        }*/
    } 
}


