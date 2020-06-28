using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageEnemyData
    {
        [SerializeField, FoldoutGroup("$m_enemyType")]
        private ENEMY_TYPE m_enemyType;
        [SerializeField, FoldoutGroup("$m_enemyType")]
        private int m_enemyCount;

        public ENEMY_TYPE EnemyType => m_enemyType;
        public int EnemyCount => m_enemyCount;
    }
}