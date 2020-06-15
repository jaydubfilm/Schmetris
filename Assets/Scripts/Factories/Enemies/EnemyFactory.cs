using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;

namespace StarSalvager.Factories
{
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        [SerializeField, Required]
        private List<EnemyTypeScriptableObject> enemyTypes;

        [SerializeField, Required]
        private EnemyProfileScriptableObject enemyProfiles;

        [SerializeField, Required]
        private GameObject enemyPrefab;

        //============================================================================================================//

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
            var profile = enemyProfiles.GetProfile(enemyType);

            EnemyData enemyData = new EnemyData(profile.EnemyID, profile.Name, profile.Health, profile.MovementSpeed, profile.AttackDamage, profile.AttackSpeed);
            
            var enemy = CreateObject<Enemy>();

            enemy.m_enemyData = enemyData;

            return enemy.GetComponent<T>();
        }

        //============================================================================================================//
    }
}

