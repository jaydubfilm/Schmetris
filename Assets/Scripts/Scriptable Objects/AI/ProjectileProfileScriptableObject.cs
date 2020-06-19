using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.AI;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Projectile_Profile", menuName = "Star Salvager/Scriptable Objects/Projectile Profile")]
    public class ProjectileProfileScriptableObject : ScriptableObject
    {
        [SerializeField, Required]
        public GameObject m_prefab;

        public List<ProjectileProfileData> m_projectileProfileData = new List<ProjectileProfileData>();

        public ProjectileProfileData GetProjectileProfileData(PROJECTILE_TYPE Type)
        {
            return m_projectileProfileData
                .FirstOrDefault(p => p.ProjectileType == Type);
        }
    }

}