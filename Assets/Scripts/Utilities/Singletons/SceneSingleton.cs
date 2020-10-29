using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities
{
    /// <summary>
    /// Used for Scene Dependent Singletons (Destoy Object on Load)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DefaultExecutionOrder(-10000)]
    public class SceneSingleton<T> : MonoBehaviour where T: Object
    {
        public static T Instance => _instance;
        private static T _instance;
        private void Awake()
        {
            if (Instance != null)
            {
                throw new Exception($"An instance of {nameof(T)} already exists.");
            }

            _instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }
}

