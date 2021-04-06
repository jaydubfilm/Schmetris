using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class EnemyRemoteData
    {
        [SerializeField, HorizontalGroup("$Name/row1"), DisplayAsString, LabelText("Enemy ID")]
        private string m_enemyType = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
        [Button("Copy"), HorizontalGroup("$Name/row1", 45)]
        private void CopyID()
        {
            GUIUtility.systemCopyBuffer = m_enemyType;
        }

        public void EditorUpdateChildren()
        {
            foreach (var rdsTableData in m_rdsTableData)
            {
                rdsTableData.EditorUpdateChildren();
            }
        }

#endif

        [FoldoutGroup("$Name")] public bool isImplemented;
        
        [SerializeField, FoldoutGroup("$Name")]
        private string m_name;

        [SerializeField, FoldoutGroup("$Name")]
        private int m_health;

        [SerializeField, FoldoutGroup("$Name")]
        private float m_movementSpeed;

        [SerializeField, FoldoutGroup("$Name")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_attackSpeed")]
        private float m_rateOfFire;

        [SerializeField, FoldoutGroup("$Name")]
        private Vector2Int m_dimensions = Vector2Int.one;

        [SerializeField, FoldoutGroup("$Name"), LabelText("Loot Drops")]
        private List<RDSTableData> m_rdsTableData;

        public string EnemyID => m_enemyType;

        public string Name => m_name;

        public int Health => m_health;

        public float MovementSpeed => m_movementSpeed;

        public float RateOfFire => m_rateOfFire;

        public Vector2Int Dimensions => m_dimensions;

        public List<RDSTableData> RDSTableData => m_rdsTableData;
    }
}