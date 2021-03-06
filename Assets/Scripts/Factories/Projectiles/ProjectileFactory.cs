﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.AI;
using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.Projectiles;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace StarSalvager.Factories
{
    public class ProjectileFactory : FactoryBase
    {
        private readonly GameObject m_prefab;
        private readonly GameObject m_towPrefab;
        
        private readonly ProjectileProfileScriptableObject m_projectileProfile;

        //============================================================================================================//

        public ProjectileFactory(ProjectileProfileScriptableObject projectileProfile)
        {
            m_projectileProfile = projectileProfile;
            m_prefab = projectileProfile.m_prefab;
            m_towPrefab = projectileProfile.m_towPrefab;
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
            if (Recycler.TryGrab(out T newObject))
            {
                return newObject;
            }

            return typeof(T) == typeof(ProjectileTowObject)
                ? Object.Instantiate(m_towPrefab).GetComponent<T>()
                : CreateGameObject().GetComponent<T>();
        }

        //Static Target position functions
        //============================================================================================================//

        //TODO: Add setting the collisionTag for the projectile
        public T[] CreateObjects<T>(string projectileType,
            Vector2 fromPosition,
            Vector2 targetPosition,
            Vector2 shootDirection,
            float rangeBoost,
            string[] collisionTags,
            IHealth vampirismCaster,
            float vampirismValue,
            bool shouldFlipSprite = false,
            bool shouldAlignToGridY = false)
        {
            return CreateObjects<T>(projectileType, fromPosition, targetPosition, Vector2.zero, shootDirection,
                rangeBoost, collisionTags, vampirismCaster, vampirismValue, shouldFlipSprite, shouldAlignToGridY);
        }
        
        public T[] CreateObjects<T>(string projectileType, 
            Vector2 fromPosition, 
            Vector2 targetPosition,
            Vector2 currentVelocity, 
            Vector2 shootDirection,
            float rangeBoost, 
            string[] collisionTags, 
            IHealth vampirismCaster,
            float vampirismValue,
            bool shouldFlipSprite = false,
            bool shouldAlignToGridY = false)
        {
            var projectiles = new List<T>();
            var projectileProfile = m_projectileProfile.GetProjectileProfileData(projectileType);

            if (shouldAlignToGridY)
            {
                fromPosition = new Vector2(
                    LevelManager.Instance.WorldGrid.GetLocalPositionOfCenterOfGridSquareAtLocalPosition(fromPosition).x,
                    fromPosition.y);
            }

            var travelDirections = GetFireDirections(projectileProfile, fromPosition, /*targetPosition,*/ shootDirection);

            foreach (var travelDirection in travelDirections)
            {
                Projectile projectile;
                if (projectileProfile.IsTow)
                {
                    ProjectileTowObject projectileTowObject = CreateObject<ProjectileTowObject>();
                    GameObject towObject;
                    switch(projectileProfile.TowObjectType)
                    {
                        case ProjectileProfileData.TowType.JunkBit:
                            towObject = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateJunkGameObject();
                            break;
                        case ProjectileProfileData.TowType.Bumper:
                            towObject = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateGameObject(BIT_TYPE.BUMPER);
                            break;
                        case ProjectileProfileData.TowType.Mine:
                            string enemyId = FactoryManager.Instance.EnemyRemoteData.GetEnemyId("Sleeper Mine");
                            
                            towObject = LevelManager.Instance.EnemyManager.SpawnEnemy(enemyId, fromPosition).gameObject;
                            break;
                        default:
                            throw new Exception("Missing data for towObject");
                    }

                    projectileTowObject.SetColliderActive(false);

                    LevelManager.Instance.ObstacleManager.AddToRoot(towObject);
                    towObject.transform.position = fromPosition;
                    
                    projectileTowObject.towObjectActor = towObject.GetComponent<Actor2DBase>();

                    projectile = projectileTowObject;
                }
                else
                {
                    projectile = CreateObject<Projectile>();
                }
                
                var projectileTransform = projectile.transform;

                projectile.SetSprite(projectileProfile.Sprite);
                

                if (shouldFlipSprite && projectileProfile.RequiresRotation)
                    projectile.FlipSpriteY(true);

                LevelManager.Instance.ObstacleManager.AddTransformToRoot(projectileTransform);
                //projectileTransform.SetParent(LevelManager.Instance.transform);
                projectileTransform.transform.position = fromPosition;

                projectile.Init(projectileProfile,
                    null,
                    collisionTags,
                    projectileProfile.ProjectileDamage,
                    rangeBoost,
                    travelDirection.normalized,
                    projectileProfile.AddVelocityToProjectiles ? currentVelocity : Vector2.zero,
                    projectileProfile.Scale,
                    vampirismCaster,
                    vampirismValue);


                LevelManager.Instance.ProjectileManager.AddProjectile(projectile);


                projectiles.Add(projectile.GetComponent<T>());
            }

            return projectiles.ToArray();
        }

        //Moving Target position Functions
        //====================================================================================================================//
        
        //TODO: Add setting the collisionTag for the projectile
        public T[] CreateObjects<T>(string projectileType,
            Vector2 fromPosition,
            CollidableBase target,
            Vector2 shootDirection,
            float damage, 
            float rangeBoost, 
            string[] collisionTags,
            IHealth vampirismCaster,
            float vampirismValue,
            bool shouldFlipSprite = false)
        {
            return CreateObjects<T>(projectileType, fromPosition, target, shootDirection, Vector2.zero, damage,
                rangeBoost, collisionTags,vampirismCaster, vampirismValue, shouldFlipSprite);
        }
        
        public T[] CreateObjects<T>(string projectileType, 
            Vector2 fromPosition, 
            CollidableBase target,
            Vector2 shootDirection,
            Vector2 currentVelocity, 
            float damage,
            float rangeBoost, 
            string[] collisionTags,
            IHealth vampirismCaster,
            float vampirismValue,
            bool shouldFlipSprite = false)
        {
            var projectiles = new List<T>();
            var projectileProfile = m_projectileProfile.GetProjectileProfileData(projectileType);

            var travelDirections =
                GetFireDirections(projectileProfile, fromPosition, /*target.transform.position,*/ shootDirection);

            foreach (var travelDirection in travelDirections)
            {
                Projectile projectile;
                if (projectileProfile.IsTow)
                {
                    projectile = CreateObject<ProjectileTowObject>();
                }
                else
                {
                    projectile = CreateObject<Projectile>();
                }
                var projectileTransform = projectile.transform;

                projectile.SetSprite(projectileProfile.Sprite);

                if (shouldFlipSprite && projectileProfile.RequiresRotation)
                    projectile.FlipSpriteY(true);

                LevelManager.Instance.ObstacleManager.AddTransformToRoot(projectileTransform);
                projectileTransform.transform.position = fromPosition;

                projectile.Init(projectileProfile,
                    target,
                    collisionTags,
                    damage,
                    rangeBoost,
                    travelDirection.normalized,
                    projectileProfile.AddVelocityToProjectiles ? currentVelocity : Vector2.zero,
                    projectileProfile.Scale,
                    vampirismCaster,
                    vampirismValue);


                LevelManager.Instance.ProjectileManager.AddProjectile(projectile);


                projectiles.Add(projectile.GetComponent<T>());
            }

            return projectiles.ToArray();
        }

        public GrenadeProjectile CreateGrenadeProjectile(in Vector3 position, in Quaternion rotation)
        {
            return Object.Instantiate(m_projectileProfile.grenadeProjectilePrefab, position, rotation);
        }


        //Static Functions
        //====================================================================================================================//

        public static ProjectileProfileData GetProfile(in string projectileId)
        {
#if UNITY_EDITOR
            return (FactoryManager.Instance == null
                ? Object.FindObjectOfType<FactoryManager>()
                : FactoryManager.Instance).ProjectileProfile.GetProjectileProfileData(projectileId);
#else
            return FactoryManager.Instance.ProjectileProfile.GetProjectileProfileData(projectileId);

#endif
        }
        
        private static IEnumerable<Vector2> GetFireDirections(ProjectileProfileData profileData, 
            Vector2 fromPosition,
            /*Vector2 targetPosition,*/
            Vector2 shootDirection)
        {
            var spreadAngle = profileData.SpreadAngle;
            var sprayCount = profileData.SprayCount;
            var fireType = profileData.FireType;

            Vector2 shootAt;

            var fireDirections = new List<Vector2>();

            switch (fireType)
            {
                //----------------------------------------------------------------------------------------------------//
                case FIRE_TYPE.FORWARD:
                case FIRE_TYPE.HEAT_SEEKING:
                    fireDirections.Add(shootDirection);
                    break;
                //----------------------------------------------------------------------------------------------------//
                /*case FIRE_TYPE.FIXED_SPRAY:
                    //Rotate player position around enemy position slightly by a random angle to shoot somewhere in a cone around the player
                    fireDirections.Add(GetDestinationForRotatePositionAroundPivot(targetPosition, fromPosition,
                        Vector3.forward * Random.Range(-spreadAngle, spreadAngle)) - (Vector3) fromPosition);
                    break;*/
                //----------------------------------------------------------------------------------------------------//
                case FIRE_TYPE.RANDOM_SPRAY:
                    shootAt = fromPosition + shootDirection;
                    //For each shot in the spray, rotate player position around enemy position slightly by a random angle to shoot somewhere in a cone around the player
                    for (var i = 0; i < sprayCount; i++)
                    {
                        fireDirections.Add(GetDestinationForRotatePositionAroundPivot(
                            shootAt,
                            fromPosition,
                            Vector3.forward * Random.Range(-spreadAngle, spreadAngle)) - (Vector3) fromPosition);
                    }

                    break;
                case FIRE_TYPE.FIXED_SPRAY:
                    shootAt = fromPosition + shootDirection;
                    var angleRate = spreadAngle / (sprayCount - 1);
                    var splitAngle = spreadAngle / 2f;
                    for (var i = 0; i < sprayCount; i++)
                    {
                        fireDirections.Add(GetDestinationForRotatePositionAroundPivot(
                            shootAt,
                            fromPosition,
                            Vector3.forward * ((i * angleRate) - splitAngle)) - (Vector3) fromPosition);
                    }
                    
                    break;
                //----------------------------------------------------------------------------------------------------//
                case FIRE_TYPE.SPIRAL:
                    //Consult spiral formula to get the angle to shoot the next shot at
                    //fireDirections.Add(GetSpiralAttackDirection(fromPosition, ref TEMP_SPIRAL));
                    throw new NotImplementedException("This needs work, consult Alex's if attempting to use this");
                //----------------------------------------------------------------------------------------------------//
                case FIRE_TYPE.FOUR_ANGLES:
                    fireDirections.Add(Vector2.left);
                    fireDirections.Add(Vector2.right);
                    fireDirections.Add(Vector2.up);
                    fireDirections.Add(Vector2.down);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case FIRE_TYPE.FOUR_ANGLES_DIAGONAL:
                    fireDirections.Add(Vector2.left + Vector2.up);
                    fireDirections.Add(Vector2.right + Vector2.up);
                    fireDirections.Add(Vector2.left + Vector2.down);
                    fireDirections.Add(Vector2.right + Vector2.down);
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(fireType), fireType, null);
                //----------------------------------------------------------------------------------------------------//
            }

            return fireDirections;
        }
        
        
        //Rotate point around pivot by angles amount
        private static Vector3 GetDestinationForRotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 direction = point - pivot;
            direction = Quaternion.Euler(angles) * direction;
            return direction + pivot;
        }

        //====================================================================================================================//
        
    }
}

