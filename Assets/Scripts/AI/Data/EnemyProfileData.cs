﻿using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using StarSalvager.Utilities.Animations;
using Unity.Collections;
using UnityEditor;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class EnemyProfileData
    {
        public string EnemyName
        {
            get
            {
#if UNITY_EDITOR
                return GetName();
#else
    return string.empty;
#endif
            }
        }

        [SerializeField, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right),
         HorizontalGroup("$EnemyName/row2", 65), VerticalGroup("$EnemyName/row2/left"), HideLabel]
        private Sprite m_sprite;

        [SerializeField, LabelText("Enemy Type"), VerticalGroup("$EnemyName/row2/right"),
         HorizontalGroup("$EnemyName/row2/right/row1"),
         ValueDropdown("GetEnemyTypes")]
        private string m_enemyTypeID;

        [SerializeField, LabelText("Prefab"), VerticalGroup("$EnemyName/row2/right")]
        private GameObject m_enemyPrefab;



        [SerializeField, VerticalGroup("$EnemyName/row2/right")]
        private bool m_isAttachable;

        [SerializeField, LabelText("Animation Controller"), FoldoutGroup("$EnemyName"),
         OnValueChanged("OnAnimationValueChanged")]
        private AnimationControllerScriptableObject m_enemyAnimationController;

        [SerializeField, FoldoutGroup("$EnemyName"), ValueDropdown("GetProjectileTypes")]
        private string m_projectileType;

        //====================================================================================================================//

        public GameObject EnemyPrefab => m_enemyPrefab;

        public string EnemyID => m_enemyTypeID;

        public Sprite Sprite => m_sprite;

        public AnimationControllerScriptableObject AnimationController => m_enemyAnimationController;

        public bool IsAttachable => m_isAttachable;

        public string ProjectileType => m_projectileType;

#if UNITY_EDITOR

        [Button("To Remote"), HorizontalGroup("$EnemyName/row2/right/row1"), EnableIf(nameof(HasRemoteDataSimple))]
        private void GoToProfileData()
        {
            var path = AssetDatabase.GetAssetPath(Object.FindObjectOfType<FactoryManager>().EnemyRemoteData);
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        private string GetName()
        {
            return HasRemoteData(out var remoteData) == false ? "NO REMOTE DATA" : remoteData.title;
        }

        private bool HasRemoteDataSimple()
        {
            var partRemoteData = Object.FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyRemoteData(EnemyID);

            return !(partRemoteData is null);
        }

        private bool HasRemoteData(out EnemyRemoteData enemyRemoteData)
        {
            enemyRemoteData = Object.FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyRemoteData(EnemyID);

            return !(enemyRemoteData is null);
        }

        private void OnAnimationValueChanged()
        {
            /*if (AnimationController == null)
            {
                m_sprite = null;
                return;
            }
            m_sprite = AnimationController.GetAnimation("Default").GetFrame(0);*/
        }

        private IEnumerable GetProjectileTypes()
        {
            var projectiles = Object.FindObjectOfType<FactoryManager>().ProjectileProfile.m_projectileProfileData.Where(
                x => x.isImplemented);

            var projectileTypes = new ValueDropdownList<string>();
            foreach (var data in projectiles)
            {
                projectileTypes.Add(data.ProjectileType, data.ProjectileTypeID);
            }

            return projectileTypes;
        }

        private static IEnumerable GetEnemyTypes()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyTypes();
        }
#endif

    }
}