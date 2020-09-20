using System;
using Sirenix.OdinInspector;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageObstacleData : StageObstacleShapeData
    {
        public ASTEROID_SIZE AsteroidSize => m_asteroidSize;
        
        private float DensityExponential => m_density * m_density * m_density;
        public float Density => DensityExponential / Globals.ObstacleDensityReductionModifier;


        //====================================================================================================================//

        [SerializeField, FoldoutGroup("$SelectionType"), ShowIf("SelectionType", SELECTION_TYPE.ASTEROID)]
        private ASTEROID_SIZE m_asteroidSize;
        [SerializeField, FoldoutGroup("$SelectionType"), Range(0, 1)]
        private float m_density;

        //====================================================================================================================//


#if UNITY_EDITOR
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
#endif

        //====================================================================================================================//
        
    }
}