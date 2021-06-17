using UnityEngine;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Animations;
using StarSalvager.Factories;
using StarSalvager.Projectiles;
using StarSalvager.Utilities.Extensions;
using System.Collections.Generic;

namespace StarSalvager
{
    public class EnemyData
    {
        public string EnemyType { get; }

        public string Name { get; }

        public int Health { get; }

        public float MovementSpeed { get; }

        public bool IsAttachable { get; }

        public float RateOfFire { get; }

        public FIRE_TYPE FireType { get; }
        public bool FireAtTarget { get; }

        public bool IgnoreObstacleAvoidance { get; }

        public string ProjectileType { get; }

        public Sprite Sprite { get; }
        public AnimationControllerScriptableObject AnimationController { get; }

        //public float OscillationsPerSecond { get; }

        //public float OscillationAngleRange { get; }

        //public float OrbitRadius { get; }

        //public float OrbitRadiusSqr => OrbitRadius * OrbitRadius;

        //public float NumberCellsDescend { get; }

        public bool AddVelocityToProjectiles { get; }

        public float SpreadAngle { get; }
        
        public int Gears { get; }

        public float SprayCount => m_sprayCount;

        private readonly int m_sprayCount;

        public Vector2Int Dimensions { get; }

        public List<int> RDSTableOdds { get; }

        public List<RDSTable> RDSTables { get; }

        public EnemyData(EnemyRemoteData enemyRemoteData, EnemyProfileData enemyProfileData)
        {
            var projectileProfileData = ProjectileFactory.GetProfile(enemyProfileData.ProjectileType);

            if (projectileProfileData != null)
            {
                FireType = projectileProfileData.FireType;
                AddVelocityToProjectiles = projectileProfileData.AddVelocityToProjectiles;
                SpreadAngle = projectileProfileData.SpreadAngle;
                m_sprayCount = projectileProfileData.SprayCount;
            }

            EnemyType                   = enemyRemoteData.EnemyID;
            Name                        = enemyRemoteData.Name;
            Health                      = enemyRemoteData.Health;
            MovementSpeed               = enemyRemoteData.MovementSpeed;
            IsAttachable                = enemyProfileData.IsAttachable;
            //AttackDamage                = enemyRemoteData.AttackDamage;
            RateOfFire                  = enemyRemoteData.RateOfFire;
            //IgnoreObstacleAvoidance     = enemyProfileData.IgnoreObstacleAvoidance;
            ProjectileType              = enemyProfileData.ProjectileType;
            Sprite                      = enemyProfileData.Sprite;
            AnimationController         = enemyProfileData.AnimationController;
            /*OscillationsPerSecond       = enemyProfileData.OscillationsPerSeconds;
            OscillationAngleRange       = enemyProfileData.OscillationAngleRange;
            OrbitRadius                 = enemyProfileData.OrbitRadius;
            NumberCellsDescend          = enemyProfileData.NumberCellsDescend;*/
            Dimensions                  = enemyRemoteData.Dimensions;

            RDSTableOdds = new List<int>();
            RDSTables = new List<RDSTable>();
            for (int i = 0; i < enemyRemoteData.RDSTableData.Count; i++)
            {
                RDSTable rdsTable = new RDSTable();
                rdsTable.SetupRDSTable(enemyRemoteData.RDSTableData[i].NumDrops,
                    enemyRemoteData.RDSTableData[i].RDSLootDatas,
                    enemyRemoteData.RDSTableData[i].EvenWeighting);

                RDSTableOdds.Add(enemyRemoteData.RDSTableData[i].DropChance);
                RDSTables.Add(rdsTable);
            }
        }

        /*public EnemyData(string enemyType, string name, int health, float movementSpeed, bool isAttachable,
            float attackDamage, float attackSpeed, ENEMY_MOVETYPE movementType, ENEMY_ATTACKTYPE attackType,
            string projectileType, Sprite sprite, float oscillationsPerSecond, float oscillationAngleRange,
            float orbitRadius, float numberCellsDescend, bool addVelocityToProjectiles, float spreadAngle,
            int sprayCount)
        {
            EnemyType = enemyType;
            Name = name;
            Health = health;
            MovementSpeed = movementSpeed;
            IsAttachable = isAttachable;
            AttackDamage = attackDamage;
            RateOfFire = attackSpeed;
            MovementType = movementType;
            AttackType = attackType;
            ProjectileType = projectileType;
            Sprite = sprite;
            OscillationsPerSecond = oscillationsPerSecond;
            OscillationAngleRange = oscillationAngleRange;
            OrbitRadius = orbitRadius;
            NumberCellsDescend = numberCellsDescend;
            AddVelocityToProjectiles = addVelocityToProjectiles;
            SpreadAngle = spreadAngle;
            m_sprayCount = sprayCount;
        }*/
    }
}