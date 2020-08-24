using UnityEngine;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Factories;

namespace StarSalvager
{
    public class EnemyData
    {
        public string EnemyType { get; }

        public string Name { get; }

        public int Health { get; }

        public float MovementSpeed { get; }

        public bool IsAttachable { get; }

        public float AttackDamage { get; }

        public float RateOfFire { get; }

        public ENEMY_MOVETYPE MovementType { get; }

        public ENEMY_ATTACKTYPE AttackType { get; }

        public bool IgnoreObstacleAvoidance { get; }

        public string ProjectileType { get; }

        public Sprite Sprite { get; }
        public AnimationControllerScriptableObject AnimationController { get; }

        public float OscillationsPerSecond { get; }

        public float OscillationAngleRange { get; }

        public float OrbitRadius { get; }

        public float OrbitRadiusSqr => OrbitRadius * OrbitRadius;

        public float NumberCellsDescend { get; }

        public bool AddVelocityToProjectiles { get; }

        public float SpreadAngle { get; }

        public float SprayCount => m_sprayCount;

        private readonly int m_sprayCount;

        public Vector2Int Dimensions { get; }

        public RDSTable rdsTable { get; }

        public EnemyData(EnemyRemoteData enemyRemoteData, EnemyProfileData enemyProfileData)
        {
            EnemyType = enemyRemoteData.EnemyType;
            Name = enemyRemoteData.Name;
            Health = enemyRemoteData.Health;
            MovementSpeed = enemyRemoteData.MovementSpeed;
            IsAttachable = enemyProfileData.IsAttachable;
            AttackDamage = enemyRemoteData.AttackDamage;
            RateOfFire = enemyRemoteData.RateOfFire;
            MovementType = enemyProfileData.MovementType;
            AttackType = enemyProfileData.AttackType;
            IgnoreObstacleAvoidance = enemyProfileData.IgnoreObstacleAvoidance;
            ProjectileType = enemyProfileData.ProjectileType;
            Sprite = enemyProfileData.Sprite;
            AnimationController = enemyProfileData.AnimationController;
            OscillationsPerSecond = enemyProfileData.OscillationsPerSeconds;
            OscillationAngleRange = enemyProfileData.OscillationAngleRange;
            OrbitRadius = enemyProfileData.OrbitRadius;
            NumberCellsDescend = enemyProfileData.NumberCellsDescend;
            AddVelocityToProjectiles = enemyProfileData.AddVelocityToProjectiles;
            SpreadAngle = enemyProfileData.SpreadAngle;
            m_sprayCount = enemyProfileData.SprayCount;
            Dimensions = enemyRemoteData.Dimensions;


            rdsTable = new RDSTable();
            rdsTable.rdsCount = enemyRemoteData.MaxDrops;
            foreach (var rdsData in enemyRemoteData.rdsEnemyData)
            {
                if (rdsData.rdsData == RDSEnemyData.TYPE.Bit)
                {
                    BlockData bitBlockData = new BlockData
                    {
                        ClassType = "Bit",
                        Type = rdsData.type,
                        Level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<BlockData>(bitBlockData, rdsData.probability, rdsData.isUniqueSpawn, rdsData.isAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSEnemyData.TYPE.Component)
                {
                    BlockData componentBlockData = new BlockData
                    {
                        ClassType = "Component",
                        Type = rdsData.type,
                    };
                    rdsTable.AddEntry(new RDSValue<BlockData>(componentBlockData, rdsData.probability, rdsData.isUniqueSpawn, rdsData.isAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSEnemyData.TYPE.Blueprint)
                {
                    TEST_Blueprint blueprintData = new TEST_Blueprint
                    {
                        name = (PART_TYPE)rdsData.type + " " + rdsData.level,
                        partType = (PART_TYPE)rdsData.type,
                        level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<TEST_Blueprint>(blueprintData, rdsData.probability, rdsData.isUniqueSpawn, rdsData.isAlwaysSpawn, true));
                }
            }
        }

        public EnemyData(string enemyType, string name, int health, float movementSpeed, bool isAttachable,
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
        }
    }
}