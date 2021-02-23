using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using UnityEngine;
using Object = UnityEngine.Object;


namespace StarSalvager.AI
{
    [Serializable]
    public class StageObstacleShapeData
    {
        public SELECTION_TYPE SelectionType => m_selectionType;
        public BIT_TYPE BitType => bitType;
        public string ShapeName => m_shapeName;
        public string Category => m_category;
        
        public int Rotation
        {
            get
            {
                switch(m_rotation)
                {
                    case -1:
                        return UnityEngine.Random.Range(0, 4);
                    default:
                        return m_rotation;
                }
            }

        }

        //====================================================================================================================//

        [SerializeField, FoldoutGroup("$SelectionType"), ValueDropdown("GetSelectionOptions")]
        protected SELECTION_TYPE m_selectionType = SELECTION_TYPE.BIT;
        
        [SerializeField, FoldoutGroup("$SelectionType"), ShowIf("SelectionType", 
             SELECTION_TYPE.SHAPE), ValueDropdown("GetShapes"), 
         InfoBox("No Name Selected", InfoMessageType.Error, VisibleIf = "HasNoSelectedName")]
        private string m_shapeName;
        
        [SerializeField, FoldoutGroup("$SelectionType"), ShowIf("SelectionType", SELECTION_TYPE.CATEGORY), ValueDropdown("GetCategories")]
        private string m_category;

        [SerializeField, FoldoutGroup("$SelectionType"), ShowIf("SelectionType", SELECTION_TYPE.BIT), ValueDropdown("GetBitSelectionOptions")]
        protected BIT_TYPE bitType;
        
        [SerializeField, FoldoutGroup("$SelectionType"), HideIf("SelectionType", SELECTION_TYPE.BUMPER), HideIf("SelectionType", SELECTION_TYPE.BIT), ValueDropdown("EnemyTypes")]
        private int m_rotation;

        //====================================================================================================================//
        


        #region Unity Editor Functions

#if UNITY_EDITOR
        protected readonly ValueDropdownList<int> EnemyTypes = new ValueDropdownList<int>
        {
            {"Random", -1},
            {"0", 0},
            {"90", 1},
            {"180", 2},
            {"270", 3}
        };
        
        protected IEnumerable<string> GetShapes()
        {
            var shapeDatas = Object.FindObjectOfType<FactoryManager>().EditorBotShapeData.GetEditorShapeData();
            List<string> shapeNames = new List<string>();

            foreach (var shapeData in shapeDatas)
            {
                shapeNames.Add(shapeData.Name);
            }

            return shapeNames;
        }

        protected IEnumerable<string> GetCategories()
        {
            return Object.FindObjectOfType<FactoryManager>().EditorBotShapeData.m_categories;
        }

        protected virtual ValueDropdownList<SELECTION_TYPE> GetSelectionOptions()
        {
            var valueDropdownItems = new ValueDropdownList<SELECTION_TYPE>
            {
                {"Shape", SELECTION_TYPE.SHAPE},
                {"Category", SELECTION_TYPE.CATEGORY},
            };

            return valueDropdownItems;
        }
        
        protected virtual ValueDropdownList<BIT_TYPE> GetBitSelectionOptions()
        {
            var valueDropdownItems = new ValueDropdownList<BIT_TYPE>
            {
                BIT_TYPE.BLUE,
                BIT_TYPE.GREEN,
                BIT_TYPE.GREY,
                BIT_TYPE.RED,
                BIT_TYPE.YELLOW,
            };

            return valueDropdownItems;
        }

        private bool HasNoSelectedName()
        {
            return string.IsNullOrEmpty(m_shapeName);
        }
        
#endif

        #endregion //Unity Editor Functions
    }
}