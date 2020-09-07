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

        private EnemyData SetupEnemyData(string enemyTypeID)
        {
            EnemyProfileData profile = m_enemyProfile.GetEnemyProfileData(enemyTypeID);
            EnemyRemoteData remoteData = m_enemyRemoteData.GetEnemyRemoteData(enemyTypeID);
            
            if(profile == null)
                throw new Exception($"No profile found for enemy ID [{enemyTypeID}]");
                

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
            return Object.Instantiate(m_prefab);
        }

        public GameObject CreateAttachableGameObject()
        {
            return Object.Instantiate(m_attachablePrefab);
        }

        public override T CreateObject<T>()
        {
            if (Recycler.TryGrab(out T newObject))
            {
                return newObject;
            }

            Type type = typeof(T);


            var enemyComponent = type == typeof(EnemyAttachable)
                ? CreateAttachableGameObject().GetComponent<T>()
                : CreateGameObject().GetComponent<T>();

            return enemyComponent;
        }

        //============================================================================================================//

        public T CreateObject<T>(string guid)
        {
            EnemyData enemyData = enemyDatas.FirstOrDefault(p => p.EnemyType == guid) ?? SetupEnemyData(guid);

            Enemy enemy = enemyData.IsAttachable ? CreateObject<EnemyAttachable>() : CreateObject<Enemy>();
            
            enemy.Init(enemyData);

            return enemy.GetComponent<T>();
        }
        
        public T CreateObjectName<T>(string enemyName)
        {
            var enemyID = m_enemyProfile.GetEnemyProfileDataByName(enemyName).EnemyID;

            return string.IsNullOrEmpty(enemyID) ? default : CreateObject<T>(enemyID);
        }


        //============================================================================================================//
    }
}

