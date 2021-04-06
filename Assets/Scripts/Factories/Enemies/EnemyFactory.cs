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
using StarSalvager.Utilities.Enemies;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    public class EnemyFactory : FactoryBase
    {
        private readonly EnemyProfileScriptableObject m_enemyProfile;
        private readonly EnemyRemoteDataScriptableObject m_enemyRemoteData;

        private List<EnemyData> enemyDatas = new List<EnemyData>();

        //============================================================================================================//

        public EnemyFactory(EnemyProfileScriptableObject enemyProfile, EnemyRemoteDataScriptableObject enemyRemoteData)
        {
            m_enemyProfile = enemyProfile;
            m_enemyRemoteData = enemyRemoteData;
        }

        //============================================================================================================//

        private EnemyData SetupEnemyData(string enemyTypeID)
        {
            EnemyProfileData profile = m_enemyProfile.GetEnemyProfileData(enemyTypeID);
            EnemyRemoteData remoteData = m_enemyRemoteData.GetEnemyRemoteData(enemyTypeID);
            
            if(profile == null)
                throw new Exception($"No profile found for enemy ID [{enemyTypeID}]");
                
            //Debug.Log($"Setting up enemy: {remoteData.Name}");

            EnemyData enemyData = new EnemyData(remoteData, profile);

            enemyDatas.Add(enemyData);

            return enemyData;
        }
        
        //============================================================================================================//

        public GameObject CreateEnemyDecoyObject()
        {
            return Recycler.TryGrab<EnemyDecoy>(out GameObject newObject)
                ? newObject
                : Object.Instantiate(m_enemyProfile.m_enemyDecoy);
        }
        
        public EnemyDecoy CreateEnemyDecoy()
        {
            return Recycler.TryGrab(out EnemyDecoy newObject)
                ? newObject
                : Object.Instantiate(m_enemyProfile.m_enemyDecoy).GetComponent<EnemyDecoy>();
        }
        
        //============================================================================================================//

        
        public override GameObject CreateGameObject()
        {
            throw new Exception("This function in enemy factory shouldn't be used, go through CreateObject<T>(string guid) instead");
        }

        public override T CreateObject<T>()
        {
            throw new Exception("This function in enemy factory shouldn't be used, go through CreateObject<T>(string guid) instead");
        }

        //============================================================================================================//

        public T CreateObject<T>(string guid) where T : MonoBehaviour
        {
            EnemyProfileData enemyProfileData = m_enemyProfile.GetEnemyProfileData(guid);
            EnemyData enemyData = enemyDatas.FirstOrDefault(p => p.EnemyType == guid) ?? SetupEnemyData(guid);

            Enemy enemy;
            if (Recycler.TryGrab(out T newObject))
            {
                enemy = newObject.GetComponent<Enemy>();
            }
            else
            {
                enemy = Object.Instantiate(enemyProfileData.EnemyPrefab).GetComponent<Enemy>();
            }
            
            enemy.Init(enemyData);

            enemy.gameObject.name = $"{enemyData.Name}";

            return enemy.GetComponent<T>();
        }
        
        public T CreateObjectName<T>(string enemyName) where T : MonoBehaviour
        {
            var enemyID = m_enemyProfile.GetEnemyProfileDataByName(enemyName).EnemyID;

            return string.IsNullOrEmpty(enemyID) ? default : CreateObject<T>(enemyID);
        }


        //============================================================================================================//
    }
}

