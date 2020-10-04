using System;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.AI;
using Recycling;
using StarSalvager.Factories.Data;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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

        public ProjectileProfileData GetProfileData(string projectileType)
        {
            return m_projectileProfile.GetProjectileProfileData(projectileType);
        }

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
        public T[] CreateObjects<T>(string projectileType, Vector2 fromPosition, Vector2 targetPosition, float damage, string collisionTag, bool shouldFlipSprite = false)
        {
            return CreateObjects<T>(projectileType, fromPosition, targetPosition, Vector2.zero, damage, collisionTag, shouldFlipSprite);
        }
        
        public T[] CreateObjects<T>(string projectileType, Vector2 fromPosition, Vector2 targetPosition,
            Vector2 currentVelocity, float damage, string collisionTag, bool shouldFlipSprite = false)
        {
            var projectiles = new List<T>();
            var projectileProfile = m_projectileProfile.GetProjectileProfileData(projectileType);

            var travelDirections = GetFireDirections(projectileProfile, fromPosition, targetPosition);

            foreach (var travelDirection in travelDirections)
            {
                var projectile = CreateObject<Projectile>();
                var projectileTransform = projectile.transform;

                projectile.MProjectileData = projectileProfile;

                projectile.SetSprite(projectileProfile.Sprite);

                if (shouldFlipSprite && projectileProfile.RequiresRotation)
                    projectile.FlipSpriteY(true);

                LevelManager.Instance.ObstacleManager.AddTransformToRoot(projectileTransform);
                //projectileTransform.SetParent(LevelManager.Instance.transform);
                projectileTransform.transform.position = fromPosition;

                projectile.Init(collisionTag, damage, travelDirection.normalized,
                    projectileProfile.AddVelocityToProjectiles ? currentVelocity : Vector2.zero);


                LevelManager.Instance.ProjectileManager.AddProjectile(projectile);


                projectiles.Add(projectile.GetComponent<T>());
            }

            return projectiles.ToArray();
        }

        //============================================================================================================//

        private static IEnumerable<Vector2> GetFireDirections(ProjectileProfileData profileData, Vector2 fromPosition,
            Vector2 targetPosition)
        {
            var spreadAngle = profileData.SpreadAngle;
            var sprayCount = profileData.SprayCount;
            var attackType = profileData.AttackType;

            var fireDirections = new List<Vector2>();

            switch (attackType)
            {
                //----------------------------------------------------------------------------------------------------//
                case ENEMY_ATTACKTYPE.Forward:
                case ENEMY_ATTACKTYPE.AtPlayer:
                    fireDirections.Add(targetPosition - fromPosition);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case ENEMY_ATTACKTYPE.AtPlayerCone:
                    //Rotate player position around enemy position slightly by a random angle to shoot somewhere in a cone around the player
                    fireDirections.Add(GetDestinationForRotatePositionAroundPivot(targetPosition, fromPosition,
                        Vector3.forward * Random.Range(-spreadAngle, spreadAngle)) - (Vector3) fromPosition);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case ENEMY_ATTACKTYPE.Down:
                    fireDirections.Add(Vector3.down);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case ENEMY_ATTACKTYPE.Random_Spray:
                    //For each shot in the spray, rotate player position around enemy position slightly by a random angle to shoot somewhere in a cone around the player
                    for (var i = 0; i < sprayCount; i++)
                    {
                        fireDirections.Add(GetDestinationForRotatePositionAroundPivot(targetPosition,
                            fromPosition,
                            Vector3.forward * Random.Range(-spreadAngle,
                                spreadAngle)) - (Vector3) fromPosition);
                    }

                    break;
                case ENEMY_ATTACKTYPE.Fixed_Spray:

                    var angleRate = spreadAngle / (sprayCount - 1);
                    var splitAngle = spreadAngle / 2f;
                    for (var i = 0; i < sprayCount; i++)
                    {
                        fireDirections.Add(GetDestinationForRotatePositionAroundPivot(targetPosition,
                            fromPosition,
                            Vector3.forward * ((i * angleRate) - splitAngle)) - (Vector3) fromPosition);
                    }
                    
                    break;
                //----------------------------------------------------------------------------------------------------//
                case ENEMY_ATTACKTYPE.Spiral:
                    //Consult spiral formula to get the angle to shoot the next shot at
                    //fireDirections.Add(GetSpiralAttackDirection(fromPosition, ref TEMP_SPIRAL));
                    throw new NotImplementedException("This needs work, consult Alex's if attempting to use this");
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
                //----------------------------------------------------------------------------------------------------//
            }

            return fireDirections;
        }

        //Get the location that enemy is firing at, then create the firing projectile from the factory
        private static Vector2 GetSpiralAttackDirection(Vector2 fromPosition, ref Vector2 spiralAttackDirection)
        {
            spiralAttackDirection =
                GetDestinationForRotatePositionAroundPivot(spiralAttackDirection + fromPosition,
                    fromPosition, Vector3.forward * 30) - (Vector3)fromPosition;

            return spiralAttackDirection;
        }
        
        //Rotate point around pivot by angles amount
        private static Vector3 GetDestinationForRotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 direction = point - pivot;
            direction = Quaternion.Euler(angles) * direction;
            return direction + pivot;
        }
        
        private static Vector3 GetDestinationForRotatePositionAroundPivotAtDistance(Vector3 point, Vector3 pivot,
            Vector3 angles, float distance)
        {
            Vector3 direction = point - pivot;
            direction.Normalize();
            direction *= distance;
            direction = Quaternion.Euler(angles) * direction;
            return direction + pivot;
        }
    }
}

