using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.AI;
using Recycling;

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
            return Recycler.TryGrab(out T newObject) ? newObject : CreateGameObject().GetComponent<T>();
        }

        //============================================================================================================//

        //TODO: Add setting the collisionTag for the projectile
        public T CreateObject<T>(string projectileType, Vector3 travelDirection, float damage, string collisionTag)
        {
            var projectile = CreateObject<Projectile>();

            projectile.m_projectileData = m_projectileProfile.GetProjectileProfileData(projectileType);
            travelDirection.Normalize();
            projectile.m_travelDirectionNormalized = travelDirection;
            projectile.SetCollisionTag(collisionTag);
            projectile.SetSprite(projectile.m_projectileData.Sprite);

            if (travelDirection == Vector3.up && projectile.m_projectileData.RequiresRotation)
                projectile.FlipSpriteY(true);

            projectile.SetDamage(damage);

            return projectile.GetComponent<T>();
        }

        //============================================================================================================//
    }
}

