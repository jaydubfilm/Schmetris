﻿using System;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    public abstract class AttachableProfileScriptableObject : ScriptableObject
    {
        [Required]
        public GameObject Prefab;

        [Required]
        public GameObject AnimatedPrefab;

        [Required]
        public GameObject ScrapyardPrefab;
    }
    
    /// <summary>
    /// Attachable Factory Profile base class allows the use of IProfile Objects, and Enums to obtain specific Profiles
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public abstract class AttachableProfileScriptableObject<T, U>  : AttachableProfileScriptableObject 
        where T: IProfile
        where U: Enum
    {
        [SerializeField]
        public T[] profiles;

        public abstract T GetProfile(U Type);
    }
}
 
