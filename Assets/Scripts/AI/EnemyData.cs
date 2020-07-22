using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.AI;

namespace StarSalvager
{
    public class EnemyData
    {
        private string m_enemyType;
        private int m_enemyID;
        private string m_name;
        private int m_health;
        private float m_movementSpeed;
        private bool m_isAttachable;
        private float m_attackDamage;
        private float m_attackSpeed;
        private ENEMY_MOVETYPE m_movementType;
        private ENEMY_ATTACKTYPE m_attackType;
        private string m_projectileType;
        private Sprite m_sprite;
        private float m_oscillationsPerSecond;
        private float m_oscillationAngleRange;
        private float m_orbitRadius;
        private float m_numberCellsDescend;
        private bool m_addVelocityToProjectiles;
        private float m_spreadAngle;
        private int m_sprayCount;
        private int m_minBitExplosionCount;
        private int m_maxBitExplosionCount;

        public EnemyData(string enemyType, int enemyID, string name, int health, float movementSpeed, bool isAttachable, float attackDamage, float attackSpeed, ENEMY_MOVETYPE movementType, ENEMY_ATTACKTYPE attackType, string projectileType, Sprite sprite, float oscillationsPerSecond, float oscillationAngleRange, float orbitRadius, float numberCellsDescend, bool addVelocityToProjectiles, float spreadAngle, int sprayCount, int minBitExplosionCount, int maxBitExplosionCount)
        {
            m_enemyType = enemyType;
            m_enemyID = enemyID;
            m_name = name;
            m_health = health;
            m_movementSpeed = movementSpeed;
            m_isAttachable = isAttachable;
            m_attackDamage = attackDamage;
            m_attackSpeed = attackSpeed;
            m_movementType = movementType;
            m_attackType = attackType;
            m_projectileType = projectileType;
            m_sprite = sprite;
            m_oscillationsPerSecond = oscillationsPerSecond;
            m_oscillationAngleRange = oscillationAngleRange;
            m_orbitRadius = orbitRadius;
            m_numberCellsDescend = numberCellsDescend;
            m_addVelocityToProjectiles = addVelocityToProjectiles;
            m_spreadAngle = spreadAngle;
            m_sprayCount = sprayCount;
            m_minBitExplosionCount = minBitExplosionCount;
            m_maxBitExplosionCount = maxBitExplosionCount;
        }

        public string EnemyType
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

        public bool IsAttachable
        {
            get => m_isAttachable;
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

        public string ProjectileType
        {
            get => m_projectileType;
        }

        public Sprite Sprite
        {
            get => m_sprite;
        }

        public float OscillationsPerSecond
        {
            get => m_oscillationsPerSecond;
        }

        public float OscillationAngleRange
        {
            get => m_oscillationAngleRange;
        }

        public float OrbitRadius
        {
            get => m_orbitRadius;
        }

        public float OrbitRadiusSqr
        {
            get => m_orbitRadius * m_orbitRadius;
        }

        public float NumberCellsDescend
        {
            get => m_numberCellsDescend;
        }

        public bool AddVelocityToProjectiles
        {
            get => m_addVelocityToProjectiles;
        }

        public float SpreadAngle
        {
            get => m_spreadAngle;
        }

        public float SprayCount
        {
            get => m_sprayCount;
        }

        public int MinBitExplosionCount
        {
            get => m_minBitExplosionCount;
        }

        public int MaxBitExplosionCount
        {
            get => m_maxBitExplosionCount;
        }
    }
}