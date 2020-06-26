using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using System.Linq;
using StarSalvager.Factories.Data;
using StarSalvager.AI;
using Recycling;
using System;

namespace StarSalvager.Factories
{
    public class EnemyFactory : FactoryBase
    {
        private readonly EnemyProfileScriptableObject m_enemyProfile;
        private readonly EnemyRemoteDataScriptableObject m_enemyRemoteData;
        private readonly GameObject m_prefab;
        private readonly GameObject m_attachablePrefab;

        private List<EnemyData> enemyDatas = new List<EnemyData>();

        //============================================================================================================//

        public EnemyFactory(EnemyProfileScriptableObject enemyProfile, EnemyRemoteDataScriptableObject enemyRemoteData)
        {
            m_enemyProfile = enemyProfile;
            m_enemyRemoteData = enemyRemoteData;
            m_prefab = m_enemyProfile.m_prefab;
            m_attachablePrefab = m_enemyProfile.m_attachablePrefab;
        }

        //============================================================================================================//

        private EnemyData SetupEnemyData(ENEMY_TYPE enemyType)
        {
            EnemyProfileData profile = m_enemyProfile.GetEnemyProfileData(enemyType);
            EnemyRemoteData remoteData = m_enemyRemoteData.GetRemoteData(enemyType);

            EnemyData enemyData = new EnemyData(remoteData.EnemyType, remoteData.EnemyID, remoteData.Name, remoteData.Health, remoteData.MovementSpeed, profile.IsAttachable, remoteData.AttackDamage, remoteData.AttackSpeed, profile.MovementType, profile.AttackType, profile.ProjectileType, profile.Sprite, profile.OscillationsPerSeconds, profile.OscillationAngleRange, profile.OrbitRadius, profile.NumberCellsDescend, profile.AddVelocityToProjectiles, profile.SpreadAngle, profile.SprayCount);

            enemyDatas.Add(enemyData);

            return enemyData;
        }

        public override GameObject CreateGameObject()
        {
            return GameObject.Instantiate(m_prefab);
        }

        public GameObject CreateAttachableGameObject()
        {
            return GameObject.Instantiate(m_attachablePrefab);
        }

        public override T CreateObject<T>()
        {
            if (Recycler.TryGrab<T>(out T newObject))
            {
                return newObject;
            }

            Type type = typeof(T);
            if (type == typeof(EnemyAttachable))
                return CreateAttachableGameObject().GetComponent<T>();
            else
                return CreateGameObject().GetComponent<T>();
        }

        //============================================================================================================//

        public T CreateObject<T>(ENEMY_TYPE enemyType)
        {
            EnemyData enemyData = enemyDatas.FirstOrDefault(p => p.EnemyType == enemyType);

            if (enemyData == null)
            {
                enemyData = SetupEnemyData(enemyType);
            }

            if (enemyData.IsAttachable)
            {
                var enemy = CreateObject<EnemyAttachable>();
                enemy.EnemyData = enemyData;
                return enemy.GetComponent<T>();
            }
            else
            {
                var enemy = CreateObject<Enemy>();
                enemy.EnemyData = enemyData;
                return enemy.GetComponent<T>();
            }
        }

        //============================================================================================================//
    }
}

