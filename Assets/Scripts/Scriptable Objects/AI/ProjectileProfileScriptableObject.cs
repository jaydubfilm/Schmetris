﻿using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Projectiles;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Projectile_Profile", menuName = "Star Salvager/Scriptable Objects/Projectile Profile")]
    public class ProjectileProfileScriptableObject : ScriptableObject
    {
        [SerializeField, Required]
        public GameObject m_prefab;

        [SerializeField, Required]
        public GameObject m_towPrefab;
        
        [SerializeField, Required]
        public GrenadeProjectile grenadeProjectilePrefab;

        //====================================================================================================================//
        
        [ListDrawerSettings(ShowPaging = false)]
        public List<ProjectileProfileData> m_projectileProfileData = new List<ProjectileProfileData>();

        public ProjectileProfileData GetProjectileProfileData(string Type)
        {
            return m_projectileProfileData
                .FirstOrDefault(p => p.ProjectileTypeID == Type);
        }
    }

}