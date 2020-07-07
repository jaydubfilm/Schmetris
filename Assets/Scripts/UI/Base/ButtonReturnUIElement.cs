﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    /// <summary>
    /// Button UI element with a specified return type.
    /// </summary>
    /// <typeparam name="T">The Data type that will be stored on this UI Element</typeparam>
    /// <typeparam name="U">The Callback return Type</typeparam>
    [RequireComponent(typeof(Button))]
    public abstract class ButtonReturnUIElement<T, U> : ButtonUIElement<T>
    {
        public abstract void Init(T data, Action<U> OnPressed);

        public sealed override void Init(T data, Action OnPressed)
        {
            Init(data);
            
            button.onClick.AddListener(() =>
            {
                OnPressed?.Invoke();
            });
        }
    } 
}
