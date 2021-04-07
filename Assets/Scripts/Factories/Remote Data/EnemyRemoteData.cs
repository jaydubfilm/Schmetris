using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

using Object = UnityEngine.Object;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class EnemyRemoteData
    {
        //Properties
        //====================================================================================================================//

        #region Public Properties

        public string EnemyID => m_enemyType;

        public string Name => m_name;

        public int Health => m_health;

        public float MovementSpeed => m_movementSpeed;

        public float RateOfFire => m_rateOfFire;

        public Vector2Int Dimensions => m_dimensions;

        public List<RDSTableData> RDSTableData => m_rdsTableData;

        #endregion //Public Properties

        [SerializeField, HorizontalGroup("$title/row1"), DisplayAsString, LabelText("Enemy ID"), PropertyOrder(-100)]
        private string m_enemyType = Guid.NewGuid().ToString();

        [FoldoutGroup("$title"), VerticalGroup("$title/row2/right"), HorizontalGroup("$title/row2/right/row1")]
        public bool isImplemented;

        [SerializeField, FoldoutGroup("$title"), VerticalGroup("$title/row2/right")]
        private string m_name;

        [SerializeField, FoldoutGroup("$Name")]
        private int cost;

        [SerializeField, FoldoutGroup("$title"), VerticalGroup("$title/row2/right")]
        private int m_health;

        [SerializeField, FoldoutGroup("$title")]
        private float m_movementSpeed;

        [SerializeField, FoldoutGroup("$title")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_attackSpeed")]
        private float m_rateOfFire;

        [SerializeField, FoldoutGroup("$title")]
        private Vector2Int m_dimensions = Vector2Int.one;

        [SerializeField, FoldoutGroup("$title"), LabelText("Loot Drops")]
        private List<RDSTableData> m_rdsTableData;

        //Unity Editor
        //====================================================================================================================//

#if UNITY_EDITOR
        public string title => $"{Name} {(isImplemented ? string.Empty : "[NOT IMPLEMENTED]")}";

        public int Cost => cost;

        public int Health => m_health;
        
        [ShowInInspector, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right),
         HorizontalGroup("$title/row2", 65), VerticalGroup("$title/row2/left"), HideLabel, PropertyOrder(-100),
         ReadOnly]
        public Sprite Sprite => !HasProfile(out var profile) ? null : profile.Sprite;

        [Button("To Profile"), HorizontalGroup("$title/row2/right/row1"), EnableIf(nameof(HasProfileSimple))]
        private void GoToProfileData()
        {
            var path = AssetDatabase.GetAssetPath(Object.FindObjectOfType<FactoryManager>().EnemyProfile);
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        [Button("Copy"), HorizontalGroup("$title/row1", 45)]
        private void CopyID()
        {
            GUIUtility.systemCopyBuffer = m_enemyType;
        }

        public void EditorUpdateChildren()
        {
            foreach (var rdsTableData in m_rdsTableData)
            {
                rdsTableData.EditorUpdateChildren();
            }
        }

        private bool HasProfileSimple()
        {
            var partProfile = Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyProfileData(m_enemyType);

            return !(partProfile is null);
        }

        private bool HasProfile(out EnemyProfileData enemyProfileData)
        {
            enemyProfileData = Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyProfileData(m_enemyType);

            return !(enemyProfileData is null);
        }
#endif

        //====================================================================================================================//

    }
}
