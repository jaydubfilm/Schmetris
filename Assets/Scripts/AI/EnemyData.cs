using UnityEngine;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities.JsonDataTypes;

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

        public float AttackSpeed { get; }

        public ENEMY_MOVETYPE MovementType { get; }

        public ENEMY_ATTACKTYPE AttackType { get; }

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

        public int MinBitExplosionCount { get; }

        public int MaxBitExplosionCount { get; }
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
            AttackSpeed = enemyRemoteData.AttackSpeed;
            MovementType = enemyProfileData.MovementType;
            AttackType = enemyProfileData.AttackType;
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
            MinBitExplosionCount = enemyRemoteData.MinBitExplosionCount;
            MaxBitExplosionCount = enemyRemoteData.MaxBitExplosionCount;
            Dimensions = enemyRemoteData.Dimensions;


            //TESTING RDS TABLES - IF YOU SEE THIS, DELETE THIS WHOLE BELOW SECTION
            rdsTable = new RDSTable();

            BlockData blockData1 = new BlockData
            {
                ClassType = "Bit",
                Type = (int)BIT_TYPE.BLUE,
                Level = 0
            };
            BlockData blockData2 = new BlockData
            {
                ClassType = "Bit",
                Type = (int)BIT_TYPE.RED,
                Level = 1
            };
            BlockData blockData3 = new BlockData
            {
                ClassType = "Component",
                Type = (int)COMPONENT_TYPE.DOHICKEY,
            };
            rdsTable.AddEntry(new RDSValue<BlockData>(blockData1, 3));
            rdsTable.AddEntry(new RDSBlockData(blockData2, 3));
            rdsTable.AddEntry(new RDSBlockData(blockData3, 3));

            rdsTable.rdsCount = 5;
        }

        public EnemyData(string enemyType, string name, int health, float movementSpeed, bool isAttachable,
            float attackDamage, float attackSpeed, ENEMY_MOVETYPE movementType, ENEMY_ATTACKTYPE attackType,
            string projectileType, Sprite sprite, float oscillationsPerSecond, float oscillationAngleRange,
            float orbitRadius, float numberCellsDescend, bool addVelocityToProjectiles, float spreadAngle,
            int sprayCount, int minBitExplosionCount, int maxBitExplosionCount)
        {
            EnemyType = enemyType;
            Name = name;
            Health = health;
            MovementSpeed = movementSpeed;
            IsAttachable = isAttachable;
            AttackDamage = attackDamage;
            AttackSpeed = attackSpeed;
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
            MinBitExplosionCount = minBitExplosionCount;
            MaxBitExplosionCount = maxBitExplosionCount;
        }
    }
}