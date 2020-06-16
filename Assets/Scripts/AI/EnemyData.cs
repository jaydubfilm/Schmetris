using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class EnemyData
    {
        private ENEMY_TYPE m_enemyType;
        private int m_enemyID;
        private string m_name;
        private int m_health;
        private float m_movementSpeed;
        private float m_attackDamage;
        private float m_attackSpeed;
        private ENEMY_MOVETYPE m_movementType;
        private ENEMY_ATTACKTYPE m_attackType;
        private Sprite m_sprite;

        public EnemyData(ENEMY_TYPE enemyType, int enemyID, string name, int health, float movementSpeed, float attackDamage, float attackSpeed, ENEMY_MOVETYPE movementType, ENEMY_ATTACKTYPE attackType, Sprite sprite)
        {
            m_enemyType = enemyType;
            m_enemyID = enemyID;
            m_name = name;
            m_health = health;
            m_movementSpeed = movementSpeed;
            m_attackDamage = attackDamage;
            m_attackSpeed = attackSpeed;
            m_movementType = movementType;
            m_attackType = attackType;
            m_sprite = sprite;
        }

        public ENEMY_TYPE EnemyType
        {
            get => m_enemyType;
        }

        public int EnemyID
        {
            get => m_enemyID;
        }

        public string Name
        {
            get => m_name;
        }

        public int Health
        {
            get => m_health;
        }

        public float MovementSpeed
        {
            get => m_movementSpeed;
        }

        public float AttackDamage
        {
            get => m_attackDamage;
        }

        public float AttackSpeed
        {
            get => m_attackSpeed;
        }

        public ENEMY_MOVETYPE MovementType
        {
            get => m_movementType;
        }

        public ENEMY_ATTACKTYPE AttackType
        {
            get => m_attackType;
        }

        public Sprite Sprite
        {
            get => m_sprite;
        }
    }
}