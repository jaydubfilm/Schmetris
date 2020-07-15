using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class EditorShapeGeneratorData : EditorGeneratorDataBase
    {
        public EditorShapeGeneratorData(string name, List<BlockData> blockData) : base(name, blockData)
        {

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