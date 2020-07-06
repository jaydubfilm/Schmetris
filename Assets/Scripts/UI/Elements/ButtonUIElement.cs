﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    [RequireComponent(typeof(Button))]
    public abstract class ButtonUIElement<T, U> : UIElement<T>
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

        private Button _button;

        public abstract void Init(T data, Action<U> OnPressed);

        public sealed override void Init(T data)
        {
            this.data = data;
        }
    } 
}


