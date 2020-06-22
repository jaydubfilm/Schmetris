using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities
{
    /// <summary>
    /// Used for session dependent singletons (Dont' destroy object on Load)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T: Object
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
            DontDestroyOnLoad(gameObject);
        }
    }
}

