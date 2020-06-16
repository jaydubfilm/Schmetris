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
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        [SerializeField, Required]
        private EnemyProfileScriptableObject m_enemyProfiles;

        [SerializeField, Required]
        private EnemyRemoteDataScriptableObject m_enemyRemoteDatas;

        [SerializeField, Required]
        private GameObject enemyPrefab;

        private List<EnemyData> enemyDatas = new List<EnemyData>();

        //============================================================================================================//

        private EnemyData SetupEnemyData(ENEMY_TYPE enemyType)
        {
            EnemyProfileData enemyProfile = m_enemyProfiles.GetEnemyProfileData(enemyType);
            EnemyRemoteData remoteData = m_enemyRemoteDatas.GetRemoteData(enemyType);

            EnemyData enemyData = new EnemyData(remoteData.EnemyType, remoteData.EnemyID, remoteData.Name, remoteData.Health, remoteData.MovementSpeed, remoteData.AttackDamage, remoteData.AttackSpeed, enemyProfile.MovementType, enemyProfile.AttackType, enemyProfile.Sprite);

            enemyDatas.Add(enemyData);

            return enemyData;
        }

        public GameObject CreateGameObject()
        {
            return GameObject.Instantiate(enemyPrefab);
        }

        public T CreateObject<T>()
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

