using System;
using Sirenix.OdinInspector;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.UIElements;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageObstacleData : StageObstacleShapeData
    {
        public ASTEROID_SIZE AsteroidSize => m_asteroidSize;
        
        //[ FoldoutGroup("$SelectionType"), ShowInInspector, DisplayAsString]
        private float DensityExponential => m_density * m_density * m_density;
        public float Density()
        {
            return DensityExponential / Globals.ObstacleDensityReductionModifier;
        }

        //====================================================================================================================//

        [SerializeField, FoldoutGroup("$SelectionType"), ShowIf("SelectionType", SELECTION_TYPE.ASTEROID)]
        private ASTEROID_SIZE m_asteroidSize;

        [SerializeField, HideInInspector]
        private float m_density;
        [SerializeField, HorizontalGroup("$SelectionType/perMinute"), Range(0, 500), PropertyTooltip("Average Per Screen Width"), LabelText("Spawns per Minute"), DisableIf("$m_maxOut"), OnValueChanged("UpdateDensity")]
        private int m_spawnsPerScreenWidthPerMinute;
        [SerializeField, HorizontalGroup("$SelectionType/perMinute"), LabelWidth(5), LabelText("Maxed"), ToggleLeft, OnValueChanged("UpdateDensity")]
        private bool m_maxOut;

        //====================================================================================================================//


#if UNITY_EDITOR
        //Todo: These are temporary values set OnValidate in StageObstacleData used for SpawnsPerScreenWidthPerMinute
        [HideInInspector]
        public float spawningMultiplier = 1;

        public void UpdateDensity()
        {
            //Needed to reverse the cubing, and ensure that we compensate for the GamUI covering 50% of the columns
            m_density = m_maxOut
                ? 1f
                : Mathf.Pow(GetDensityFromSpawnsPerScreenWidthPerMinute(), 0.315f) / Constants.VISIBLE_GAME_AREA;
        }

        protected override ValueDropdownList<SELECTION_TYPE> GetSelectionOptions()
        {
            var valueDropdownItems = new ValueDropdownList<SELECTION_TYPE>
            {
                {"Asteroid", SELECTION_TYPE.ASTEROID},
                {"Bumper", SELECTION_TYPE.BUMPER},
                {"Category", SELECTION_TYPE.CATEGORY},
                {"Shape", SELECTION_TYPE.SHAPE},
            };

            return valueDropdownItems;
        }

        /*protected int GetSpawnsPerScreenWidthPerMinute()
        {
            float rowsPerMinute = 60.0f / Globals.TimeForAsteroidToFallOneSquare;
            float columnWidth = Globals.ColumnsOnScreen;
            float spawningDensityPerSpawningRegionRow = Density() * spawningMultiplier;

            float spawnsPerColumn = spawningDensityPerSpawningRegionRow * columnWidth * rowsPerMinute;

            return Mathf.RoundToInt(spawnsPerColumn);
        }*/
        protected float GetDensityFromSpawnsPerScreenWidthPerMinute()
        {
            float rowsPerMinute = 60.0f / Globals.TimeForAsteroidToFallOneSquare;
            float columnWidth = Globals.ColumnsOnScreen;

            float density = m_spawnsPerScreenWidthPerMinute / (spawningMultiplier * columnWidth * rowsPerMinute);

            return density;
        }
#endif

        //====================================================================================================================//

    }
}