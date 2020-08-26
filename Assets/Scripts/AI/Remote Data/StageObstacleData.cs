using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField, FoldoutGroup("$m_selectionType"), ShowIf("m_selectionType", SELECTION_TYPE.ASTEROID)]
        private ASTEROID_SIZE m_asteroidSize;
        [SerializeField, FoldoutGroup("$m_selectionType")]
        [FormerlySerializedAs("m_asteroidCountPerMinute")]
        private int m_countPerMinute;
        [SerializeField, FoldoutGroup("$m_selectionType"), HideIf("m_selectionType", SELECTION_TYPE.BUMPER), ValueDropdown("GetRotations")]
        private string m_rotation;

        public SELECTION_TYPE SelectionType => m_selectionType;
        public string ShapeName => m_shapeName;
        public string Category => m_category;
        public ASTEROID_SIZE AsteroidSize => m_asteroidSize;
        public int CountPerMinute => m_countPerMinute;

        public float CountPerRowAverage => (m_countPerMinute / 60.0f) * Globals.TimeForAsteroidToFallOneSquare;

        public int Rotation()
        {
            int rotations = 0;
            switch(m_rotation)
            {
                case "Random":
                    rotations = UnityEngine.Random.Range(0, 4);
                    break;
                case "0":
                    rotations = 0;
                    break;
                case "1":
                    rotations = 1;
                    break;
                case "2":
                    rotations = 2;
                    break;
                case "3":
                    rotations = 3;
                    break;
            }

            return rotations;
        }

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

        private ValueDropdownList<string> GetRotations()
        {
            ValueDropdownList<string> enemyTypes = new ValueDropdownList<string>();
            enemyTypes.Add("Random");
            enemyTypes.Add("0");
            enemyTypes.Add("90");
            enemyTypes.Add("180");
            enemyTypes.Add("270");
            return enemyTypes;
        }
    }
}