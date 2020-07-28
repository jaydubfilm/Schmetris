using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [System.Serializable]
    public struct MissionUnlockCheckData
    {
        public string ClassType;

        public bool IsComplete;

        public string MissionName;
        public int SectorNumber;
        public int WaveNumber;
    }
}
