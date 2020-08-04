using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageObstacleData
    {
        [SerializeField, FoldoutGroup("$m_selectionType")]
        private SELECTION_TYPE m_selectionType;
        [SerializeField, FoldoutGroup("$m_selectionType"), ShowIf("m_selectionType", SELECTION_TYPE.SHAPE), ValueDropdown("GetShapes")]
        private string m_shapeName;
        [SerializeField, FoldoutGroup("$m_selectionType"), ShowIf("m_selectionType", SELECTION_TYPE.CATEGORY), ValueDropdown("GetCatgories")]
        private string m_category;
        [SerializeField, FoldoutGroup("$m_selectionType")]
        private int m_asteroidCountPerMinute;

        public SELECTION_TYPE SelectionType => m_selectionType;
        public string ShapeName => m_shapeName;
        public string Category => m_category;
        public int AsteroidCountPerMinute => m_asteroidCountPerMinute;

        public float AsteroidPerRowAverage => (m_asteroidCountPerMinute / 60.0f) * Constants.timeForAsteroidsToFall;

        private IEnumerable<string> GetShapes()
        {
            var shapeDatas = GameObject.FindObjectOfType<FactoryManager>().EditorBotShapeData.GetEditorShapeData();
            List<string> shapeNames = new List<string>();

            foreach (var shapeData in shapeDatas)
            {
                shapeNames.Add(shapeData.Name);
            }

            return shapeNames;
        }

        private IEnumerable<string> GetCatgories()
        {
            return GameObject.FindObjectOfType<FactoryManager>().EditorBotShapeData.m_categories;
        }
    }
}