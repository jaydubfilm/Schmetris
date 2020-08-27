using System;
using System.Collections.Generic;
using StarSalvager.Utilities.JsonDataTypes;

namespace StarSalvager.Utilities.Analytics
{
    [Serializable]
    public struct SessionData
    {
        public string id;
        public DateTime date;
        public List<WaveData> waves;

    }
    
    [Serializable]
    public struct WaveData
    {
        public DateTime date;
        public float timeIn;
        public int waveNumber;
        public int sectorNumber;
        public bool playerWasKilled;

        public int bumpersHit;
        public float totalDamageReceived;
        
        
        public List<BlockData> botAtStart;
        public List<BlockData> botAtEnd;

        public Dictionary<BIT_TYPE, int> liquidProcessed;
        public Dictionary<BIT_TYPE, int> bitsCollected;
        public Dictionary<BIT_TYPE, int> bitsDisconnected;
        public Dictionary<COMPONENT_TYPE, int> componentsCollected;

        public Dictionary<string, int> enemiesKilled;
        //TODO Need to add value for combos
        
    }



    /*public struct PlayerData
    {
        public string id;
        public List<SessionData> sessions;
    }*/
}
