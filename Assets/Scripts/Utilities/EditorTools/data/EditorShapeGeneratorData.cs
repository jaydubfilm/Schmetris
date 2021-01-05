using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    public class EditorShapeGeneratorData : EditorGeneratorDataBase
    {
        [SerializeField, BoxGroup("Name")]
        private List<string> m_categories;
        public List<string> Categories => m_categories;
        
        public EditorShapeGeneratorData(string name, List<IBlockData> blockData, List<string> categories) : base(name, blockData)
        {
            m_categories = categories;
            m_classType = nameof(EditorShapeGeneratorData);
        }


        public ASTEROID_SIZE AsteroidSize()
        {
            switch (BlockData.Count)
            {
                case 1:
                    return ASTEROID_SIZE.Bit;
                case 2:
                case 3:
                    return ASTEROID_SIZE.Small;
                case 4:
                case 5:
                    return ASTEROID_SIZE.Medium;
                case 6:
                case 7:
                case 8:
                case 9:
                default:
                    return ASTEROID_SIZE.Large;
            }
        }
    }
}