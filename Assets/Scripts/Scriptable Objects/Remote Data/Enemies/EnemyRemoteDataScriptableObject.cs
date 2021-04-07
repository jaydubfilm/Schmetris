using System;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using StarSalvager.AI;
using System.Collections;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using StarSalvager.Factories;
using UnityEditor;
#endif

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Enemy_Remote", menuName = "Star Salvager/Scriptable Objects/Enemy Remote Data")]
    public class EnemyRemoteDataScriptableObject : ScriptableObject
    {
        public List<EnemyRemoteData> m_enemyRemoteData = new List<EnemyRemoteData>();

        public EnemyRemoteData GetEnemyRemoteData(string TypeID)
        {
            return m_enemyRemoteData
                .FirstOrDefault(p => p.EnemyID == TypeID);
        }
        public EnemyRemoteData GetEnemyRemoteDataByName(string enemyName)
        {
            return m_enemyRemoteData
                .FirstOrDefault(p => p.Name.Equals(enemyName));
        }

        public string GetEnemyId(string name)
        {
            return GetEnemyRemoteDataByName(name)?.EnemyID;
        }
        
#if UNITY_EDITOR

        /*public void OnEnable()
        {
            Selection.selectionChanged += EditorOnSelectionChanged;
            
            foreach (var remoteData in m_enemyRemoteData)
            {
                remoteData.EditorUpdateChildren();
            }
        }

        public void OnDisable()
        {
            Selection.selectionChanged -= EditorOnSelectionChanged;
        }

        private void EditorOnSelectionChanged()
        {
            var objects = Selection.objects;
            
            if (objects.Length != 1)
                return;
            
            if (!(objects[0] is EnemyRemoteDataScriptableObject enemyRemoteData))
                return;

            foreach (var remoteData in enemyRemoteData.m_enemyRemoteData)
            {
                remoteData.EditorUpdateChildren();
            }
            
            
        }*/

        /*
        public string GetEnemyName(string id)
        {
            return GetEnemyRemoteData(id)?.Name;
        }*/

        

        public static IEnumerable GetEnemyTypes()
        {
            var enemyRemoteData = FindObjectOfType<FactoryManager>().EnemyRemoteData.m_enemyRemoteData;
            
            var enemyNamesIds = enemyRemoteData.Select(x => (x.Name, x.EnemyID)).ToList();

            var enemyTypes = new ValueDropdownList<string>();
            foreach (var (enemyName, enemyID) in enemyNamesIds)
            {
                enemyTypes.Add(enemyName, enemyID);
            }
            
            return enemyTypes;
        }

        [Button, PropertyOrder(-100)]
        private void SaveData()
        {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

#endif

    }

}