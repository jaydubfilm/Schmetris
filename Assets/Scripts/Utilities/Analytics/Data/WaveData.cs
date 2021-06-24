using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Analytics.Data;
using StarSalvager.Utilities.JSON.Converters;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Puzzle.Structs;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.SessionTracking.Data
{
    [Serializable]
    public struct WaveData
    {
        [JsonIgnore] public string Title => $"Ring {ringIndex + 1} - Wave {waveNumber + 1}";
        [JsonIgnore] public string Date => $"{date}";
        [JsonIgnore] public string TimeIn => $"{timeIn:#.00}s";

        //Properties
        //====================================================================================================================//

        #region Properties

        [HideInInspector]
        public int waveNumber;
        [HideInInspector]
        public int ringIndex;
        
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

        [DisplayAsString]
        public int xpEarned;
        [DisplayAsString]
        public int gearsCollected;
        [DisplayAsString]
        public int silverEarned;
        
        [HorizontalGroup("Row1"), ShowInInspector]
        public List<IBlockData> botAtStart;
        [HorizontalGroup("Row1"), ShowInInspector]
        public List<IBlockData> botAtEnd;

        [TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<BitSummaryData> BitSummaryData;
        
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<EnemySummaryData> enemiesKilledData;
        [JsonProperty, JsonConverter(typeof(ComboRecordDataConverter))]
        public Dictionary<ComboRecordData, int> CombosMade;

        #endregion //Properties


        //Wreck Properties
        //====================================================================================================================//

        public bool isWreck;
        public Vector2Int wreckCoordinates;

        public PartSelectionData SelectedPart;
        public PartSelectionData DiscardedPart;
        public List<PartData> purchasedPatches;

        public int spentGears;
        public int spentSilver;
        
        //Constructor
        //====================================================================================================================//

        public WaveData(in List<IBlockData> botAtStart, in int ringIndex, in int waveNumber)
        {
            this.botAtStart = botAtStart;
            this.ringIndex = ringIndex;
            this.waveNumber = waveNumber;

            bumpersHit = 0;
            totalDamageReceived = 0;
            timeIn = 0;

            xpEarned = 0;
            gearsCollected = 0;
            silverEarned = 0;

            playerWasKilled = false;

            date = DateTime.UtcNow;

            botAtEnd = new List<IBlockData>();
            BitSummaryData = new List<BitSummaryData>();
            enemiesKilledData = new List<EnemySummaryData>();
            CombosMade = new Dictionary<ComboRecordData, int>();

            isWreck = false;
            wreckCoordinates = Vector2Int.zero;
            SelectedPart = PartSelectionData.Empty;
            DiscardedPart = PartSelectionData.Empty;
            purchasedPatches = default;
            spentGears = 0;
            spentSilver = 0;
        }
        
        public WaveData(in bool isWreck, in Vector2Int wreckCoordinates)
        {
            if (!isWreck) 
                throw new ArgumentException();
            
            this.isWreck = isWreck;
            this.wreckCoordinates = Vector2Int.zero;
            SelectedPart = PartSelectionData.Empty;
            DiscardedPart = PartSelectionData.Empty;
            purchasedPatches = default;
            spentGears = 0;
            spentSilver = 0;
            
            //--------------------------------------------------------------------------------------------------------//
            
            botAtStart = default;
            ringIndex = 0;
            waveNumber = 0;

            bumpersHit = 0;
            totalDamageReceived = 0;
            timeIn = 0;

            xpEarned = 0;
            gearsCollected = 0;
            silverEarned = 0;

            playerWasKilled = false;

            date = DateTime.UtcNow;

            botAtEnd = new List<IBlockData>();
            BitSummaryData = new List<BitSummaryData>();
            enemiesKilledData = new List<EnemySummaryData>();
            CombosMade = new Dictionary<ComboRecordData, int>();


        }

        //====================================================================================================================//
        
    }
    
    

    
    
    

    

    


}
