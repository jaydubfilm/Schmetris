using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using System.Linq;
using StarSalvager.Factories.Data;

namespace StarSalvager.Factories
{
    public class EnemyFactory : FactoryBase
    {
        private readonly EnemyProfileScriptableObject m_enemyProfile;
        private readonly EnemyRemoteDataScriptableObject m_enemyRemoteData;
        private readonly GameObject m_prefab;

        private List<EnemyData> enemyDatas = new List<EnemyData>();

        //============================================================================================================//

        public EnemyFactory(EnemyProfileScriptableObject enemyProfile, EnemyRemoteDataScriptableObject enemyRemoteData)
        {
            m_enemyProfile = enemyProfile;
            m_enemyRemoteData = enemyRemoteData;
            m_prefab = m_enemyProfile.m_prefab;
        }

        //============================================================================================================//

        private EnemyData SetupEnemyData(ENEMY_TYPE enemyType)
        {
            EnemyProfileData profile = m_enemyProfile.GetEnemyProfileData(enemyType);
            EnemyRemoteData remoteData = m_enemyRemoteData.GetRemoteData(enemyType);

            EnemyData enemyData = new EnemyData(remoteData.EnemyType, remoteData.EnemyID, remoteData.Name, remoteData.Health, remoteData.MovementSpeed, remoteData.AttackDamage, remoteData.AttackSpeed, profile.MovementType, profile.AttackType, profile.Sprite, profile.OscillationsPerSeconds, profile.OscillationAngleRange, profile.OrbitRadius, profile.AtPlayerConeAngle);

            enemyDatas.Add(enemyData);

            return enemyData;
        }

        public override GameObject CreateGameObject()
        {
            return GameObject.Instantiate(m_prefab);
        }

        public override T CreateObject<T>()
        {
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

            var enemy = CreateObject<Enemy>();

            enemy.m_enemyData = enemyData;

            return enemy.GetComponent<T>();
        }

        //============================================================================================================//
    }
}

