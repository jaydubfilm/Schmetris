﻿using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;
using System.Collections.Generic;
using System;
using System.Collections;
using StarSalvager.Utilities.Animations;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class EnemyProfileData
    {
        [SerializeField, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right), HorizontalGroup("$GetEnemyType/row2", 65), VerticalGroup("$GetEnemyType/row2/left"), HideLabel]
        private Sprite m_sprite;

        [SerializeField, VerticalGroup("$GetEnemyType/row2/right"), ValueDropdown("GetEnemyTypes")]
        private string m_enemyTypeID;

        [SerializeField, VerticalGroup("$GetEnemyType/row2/right")]
        private ENEMY_MOVETYPE m_movementType;

        [SerializeField, VerticalGroup("$GetEnemyType/row2/right")]
        private bool m_isAttachable;
        
        [SerializeField, FoldoutGroup("$GetEnemyType"), OnValueChanged("OnAnimationValueChanged")]
        private AnimationControllerScriptableObject m_enemyAnimationController;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private ENEMY_ATTACKTYPE m_attackType;
        
        [SerializeField, FoldoutGroup("$GetEnemyType"), ValueDropdown("GetProjectileTypes")]
        private string m_projectileType;

        //Variables that are only shown based on the EnemyType
        private bool showOscillationsPerSecond => m_movementType == ENEMY_MOVETYPE.Oscillate || m_movementType == ENEMY_MOVETYPE.OscillateHorizontal;
        [SerializeField, FoldoutGroup("$GetEnemyType"), ShowIf("showOscillationsPerSecond")]
        private float m_oscillationsPerSeconds;

        private bool showOscillationAngleRange => m_movementType == ENEMY_MOVETYPE.Oscillate || m_movementType == ENEMY_MOVETYPE.OscillateHorizontal;
        [SerializeField, FoldoutGroup("$GetEnemyType"), ShowIf("showOscillationAngleRange")]
        private float m_oscillationAngleRange;

        [SerializeField, FoldoutGroup("$GetEnemyType"), ShowIf("m_movementType", ENEMY_MOVETYPE.Orbit)]
        private float m_orbitRadius;

        [SerializeField, FoldoutGroup("$GetEnemyType"), ShowIf("m_movementType", ENEMY_MOVETYPE.HorizontalDescend)]
        private float m_numberCellsDescend;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private bool m_ignoreObstacleAvoidance;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private bool m_addVelocityToProjectiles;

        private bool showSpreadAngle => m_attackType == ENEMY_ATTACKTYPE.AtPlayerCone || m_attackType == ENEMY_ATTACKTYPE.Spray;
        [SerializeField, FoldoutGroup("$GetEnemyType"), ShowIf("showSpreadAngle")]
        private float m_spreadAngle;

        [SerializeField, FoldoutGroup("$GetEnemyType"), ShowIf("m_attackType", ENEMY_ATTACKTYPE.Spray)]
        private int m_sprayCount;



        public string EnemyName
        {
            get
            {
#if UNITY_EDITOR
                
                return GetEnemyType();
#else
                return string.Empty;

#endif
            }
        }

        public string EnemyID => m_enemyTypeID;

        public Sprite Sprite => m_sprite;
        
        public AnimationControllerScriptableObject AnimationController => m_enemyAnimationController;

        public ENEMY_MOVETYPE MovementType => m_movementType;

        public bool IsAttachable => m_isAttachable;

        public ENEMY_ATTACKTYPE AttackType => m_attackType;

        public string ProjectileType => m_projectileType;

        public float OscillationsPerSeconds => m_oscillationsPerSeconds;

        public float OscillationAngleRange => m_oscillationAngleRange;

        public float OrbitRadius => m_orbitRadius;

        public float NumberCellsDescend => m_numberCellsDescend;

        public bool IgnoreObstacleAvoidance => m_ignoreObstacleAvoidance;

        public bool AddVelocityToProjectiles => m_addVelocityToProjectiles;

        public float SpreadAngle => m_spreadAngle;

        public int SprayCount => m_sprayCount;

        #if UNITY_EDITOR

        private void OnAnimationValueChanged()
        {
            if (AnimationController == null)
            {
                m_sprite = null;
                return;
            }
            m_sprite = AnimationController.GetAnimation("Default").GetFrame(0);
        }
        
        private IEnumerable GetProjectileTypes()
        {
            ValueDropdownList<string> projectileTypes = new ValueDropdownList<string>();
            foreach (ProjectileProfileData data in Object.FindObjectOfType<FactoryManager>().ProjectileProfile.m_projectileProfileData)
            {
                projectileTypes.Add(data.ProjectileType, data.ProjectileTypeID);
            }
            return projectileTypes;
        }

        private string GetEnemyType()
        {
            return UnityEngine.Object.FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyName(EnemyID);
        }

        private static IEnumerable GetEnemyTypes()
        {
            return UnityEngine.Object.FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyTypes();
        }
#endif

    }
}