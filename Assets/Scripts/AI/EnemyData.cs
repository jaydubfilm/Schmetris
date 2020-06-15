using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class EnemyData
    {
        private int m_enemyID;
        private string m_name;
        private int m_health;
        private float m_movementSpeed;
        private float m_attackDamage;
        private float m_attackSpeed;

        public EnemyData(int enemyID, string name, int health, float movementSpeed, float attackDamage, float attackSpeed)
        {
            m_enemyID = enemyID;
            m_name = name;
            m_health = health;
            m_movementSpeed = movementSpeed;
            m_attackDamage = attackDamage;
            m_attackSpeed = attackSpeed;
        }

        public int GetEnemyID()
        {
            return m_enemyID;
        }

        public string GetName()
        {
            return m_name;
        }

        public int GetHealth()
        {
            return m_health;
        }

        public float GetMovementSpeed()
        {
            return m_movementSpeed;
        }

        public float GetAttackDamage()
        {
            return m_attackDamage;
        }

        public float GetAttackSpeed()
        {
            return m_attackSpeed;
        }
    }
}