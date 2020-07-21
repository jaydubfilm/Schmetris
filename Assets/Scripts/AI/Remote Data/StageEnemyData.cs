using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageEnemyData
    {
        [SerializeField, FoldoutGroup("$m_enemyType")]
        private string m_enemyType;
        [SerializeField, FoldoutGroup("$m_enemyType")]
        private int m_enemyCount;

        public string EnemyType => m_enemyType;
        public int EnemyCount => m_enemyCount;
    }
}