using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;
using System.Collections.Generic;
using System;
using System.Collections;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class EnemyProfileData
    {
        [SerializeField, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right), HorizontalGroup("$EnemyType/row2", 65), VerticalGroup("$EnemyType/row2/left"), HideLabel]
        private Sprite m_sprite;

        [SerializeField, VerticalGroup("$EnemyType/row2/right")]
        private string m_enemyType;

        [SerializeField, VerticalGroup("$EnemyType/row2/right")]
        private ENEMY_MOVETYPE m_movementType;

        [SerializeField, VerticalGroup("$EnemyType/row2/right")]
        private bool m_isAttachable;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private ENEMY_ATTACKTYPE m_attackType;
        
        [SerializeField, FoldoutGroup("$EnemyType"), ValueDropdown("GetProjectileTypes")]
        private string m_projectileType;

        //Variables that are only shown based on the EnemyType
        private bool showOscillationsPerSecond => m_movementType == ENEMY_MOVETYPE.Oscillate || m_movementType == ENEMY_MOVETYPE.OscillateHorizontal;
        [SerializeField, FoldoutGroup("$EnemyType"), ShowIf("showOscillationsPerSecond")]
        private float m_oscillationsPerSeconds;

        private bool showOscillationAngleRange => m_movementType == ENEMY_MOVETYPE.Oscillate || m_movementType == ENEMY_MOVETYPE.OscillateHorizontal;
        [SerializeField, FoldoutGroup("$EnemyType"), ShowIf("showOscillationAngleRange")]
        private float m_oscillationAngleRange;

        [SerializeField, FoldoutGroup("$EnemyType"), ShowIf("m_movementType", ENEMY_MOVETYPE.Orbit)]
        private float m_orbitRadius;

        [SerializeField, FoldoutGroup("$EnemyType"), ShowIf("m_movementType", ENEMY_MOVETYPE.HorizontalDescend)]
        private float m_numberCellsDescend;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private bool m_addVelocityToProjectiles;

        private bool showSpreadAngle => m_attackType == ENEMY_ATTACKTYPE.AtPlayerCone || m_attackType == ENEMY_ATTACKTYPE.Spray;
        [SerializeField, FoldoutGroup("$EnemyType"), ShowIf("showSpreadAngle")]
        private float m_spreadAngle;

        [SerializeField, FoldoutGroup("$EnemyType"), ShowIf("m_attackType", ENEMY_ATTACKTYPE.Spray)]
        private int m_sprayCount;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private string m_enemyTypeID = System.Guid.NewGuid().ToString();

        public string EnemyType
        {
            get => m_enemyType;
        }

        public string EnemyTypeID
        {
            get => m_enemyTypeID;
        }

        public Sprite Sprite
        {
            get => m_sprite;
        }

        public ENEMY_MOVETYPE MovementType
        {
            get => m_movementType;
        }

        public bool IsAttachable
        {
            get => m_isAttachable;
        }

        public ENEMY_ATTACKTYPE AttackType
        {
            get => m_attackType;
        }

        public string ProjectileType
        {
            get => m_projectileType;
        }

        public float OscillationsPerSeconds
        {
            get => m_oscillationsPerSeconds;
        }

        public float OscillationAngleRange
        {
            get => m_oscillationAngleRange;
        }

        public float OrbitRadius
        {
            get => m_orbitRadius;
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

        public int SprayCount
        {
            get => m_sprayCount;
        }

        private IEnumerable GetProjectileTypes()
        {
            ValueDropdownList<string> projectileTypes = new ValueDropdownList<string>();
            foreach (ProjectileProfileData data in GameObject.FindObjectOfType<FactoryManager>().ProjectileProfile.m_projectileProfileData)
            {
                projectileTypes.Add(data.ProjectileType, data.ProjectileTypeID);
            }
            return projectileTypes;
        }
    }
}