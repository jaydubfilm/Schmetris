using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;

namespace StarSalvager.Factories
{
    public class ProjectileFactory : FactoryBase
    {
        private readonly GameObject m_prefab;
        private readonly ProjectileProfileScriptableObject m_projectileProfile;

        //============================================================================================================//

        public ProjectileFactory(ProjectileProfileScriptableObject projectileProfile)
        {
            m_projectileProfile = projectileProfile;
            m_prefab = projectileProfile.m_prefab;
        }

        //============================================================================================================//

        public override GameObject CreateGameObject()
        {
            return Object.Instantiate(m_prefab);
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }

        //============================================================================================================//

        public T CreateObject<T>(PROJECTILE_TYPE projectileType, Vector3 travelDirection, string collisionTag)
        {
            var projectile = CreateObject<Projectile>();

            projectile.m_projectileData = m_projectileProfile.GetProjectileProfileData(projectileType);
            projectile.m_collisionTag = collisionTag;
            travelDirection.Normalize();
            projectile.m_travelDirectionNormalized = travelDirection;

            return projectile.GetComponent<T>();
        }

        //============================================================================================================//
    }
}

