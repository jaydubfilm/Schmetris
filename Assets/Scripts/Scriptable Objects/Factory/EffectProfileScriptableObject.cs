﻿using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Effect_Profile", menuName = "Star Salvager/Scriptable Objects/Effect Profile")]
    public class EffectProfileScriptableObject : ScriptableObject
    {
        //Bot Effect Prefabs
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Bot")]
        public GameObject shieldPrototypePrefab;
        [SerializeField, Required, BoxGroup("Bot")]
        public GameObject alertIconPrefab;

        //Damage Effect Prefabs
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Damage")]
        public GameObject damageEffectPrefab;

        //Effect Prefabs
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject weldEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject impactEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject thrustEffectPrefab;

        //Particle Effect Prefabs
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject explosionPrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject labelPrefab;

        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject floatingTextPrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject magnetIconSpritePrefab;
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject fadeSpritePrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject lineShrinkPrefab;

        //====================================================================================================================//
        
    }
}