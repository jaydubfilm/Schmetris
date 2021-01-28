using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities.Analytics.Data
{
    [Serializable]
    public struct WaveData
    {
        [JsonIgnore] public string Title => $"Sector {sectorNumber + 1} - Wave {waveNumber + 1}";
        [JsonIgnore] public string Date => $"{date}";
        [JsonIgnore] public string TimeIn => $"{timeIn:#.00}s";
        
        [HideInInspector]
        public int waveNumber;
        [HideInInspector]
        public int sectorNumber;
        
        [HideInInspector]
        public DateTime date;
        
        [Title("$Title", "$Date"), ShowInInspector, DisplayAsString]
        public float timeIn;
        
        [DisplayAsString]
        public bool playerWasKilled;

        [DisplayAsString]
        public int bumpersHit;
        [DisplayAsString]
        public float totalDamageReceived;
        
        [HorizontalGroup("Row1")]
        public List<IBlockData> botAtStart;
        [HorizontalGroup("Row1")]
        public List<IBlockData> botAtEnd;

        [TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<BitSummaryData> BitSummaryData;
        
        [TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<ComponentSummaryData> ComponentSummaryData;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<EnemySummaryData> enemiesKilledData;
        //TODO Need to add value for combos

        public WaveData(List<IBlockData> botAtStart, int sectorNumber, int waveNumber)
        {
            this.botAtStart = botAtStart;
            this.sectorNumber = sectorNumber;
            this.waveNumber = waveNumber;

            bumpersHit = 0;
            totalDamageReceived = 0;
            timeIn = 0;
            
            playerWasKilled = false;
            
            date = DateTime.UtcNow;

            botAtEnd = new List<IBlockData>();
            BitSummaryData = new List<BitSummaryData>();
            ComponentSummaryData = new List<ComponentSummaryData>();
            enemiesKilledData = new List<EnemySummaryData>();
        }
        
    }
    
    

    
    
    

    

    


}
