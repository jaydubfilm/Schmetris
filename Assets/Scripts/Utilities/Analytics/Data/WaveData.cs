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
        [JsonIgnore] public string Title => isWreck ? $"Wreck {wreckCoordinates}" : $"Ring {ringIndex + 1} - Wave {waveNumber + 1}";
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
        
        [DisplayAsString, HideIf("$isWreck")]
        public bool playerWasKilled;
        [DisplayAsString, HideIf("$isWreck")]
        public int bumpersHit;
        [DisplayAsString, HideIf("$isWreck")]
        public float totalDamageReceived;

        [DisplayAsString, HideIf("$isWreck")]
        public int xpEarned;
        [DisplayAsString, HideIf("$isWreck")]
        public int gearsCollected;
        [DisplayAsString, HideIf("$isWreck")]
        public int silverEarned;
        
        [HorizontalGroup("Row1"), HideIf("$isWreck"), ShowInInspector]
        public List<IBlockData> botAtStart;
        [HorizontalGroup("Row1"), HideIf("$isWreck"), ShowInInspector]
        public List<IBlockData> botAtEnd;

        [TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true), HideIf("$isWreck")]
        public List<BitSummaryData> BitSummaryData;
        [TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true), HideIf("$isWreck")]
        public List<ComboSummaryData> comboSummaryData;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true), HideIf("$isWreck")]
        public List<EnemySummaryData> enemiesKilledData;

        #endregion //Properties


        //Wreck Properties
        //====================================================================================================================//

        [HideInInspector]
        public bool isWreck;
        [ShowIf("$isWreck"), DisplayAsString]
        public Vector2Int wreckCoordinates;

        [ShowIf("$isWreck"), BoxGroup("SelectedPart"), HideLabel]
        public PartSelectionData SelectedPart;
        [ShowIf("$isWreck"), BoxGroup("DiscardedPart"), HideLabel]
        public PartSelectionData DiscardedPart;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true), ShowIf("$isWreck")]
        public List<PartData> purchasedPatches;

        [ShowIf("$isWreck"), DisplayAsString]
        public int spentGears;
        [ShowIf("$isWreck"), DisplayAsString]
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
            comboSummaryData = new List<ComboSummaryData>();

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
            this.wreckCoordinates = wreckCoordinates;
            SelectedPart = PartSelectionData.Empty;
            DiscardedPart = PartSelectionData.Empty;
            purchasedPatches = new List<PartData>();
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
            comboSummaryData = new List<ComboSummaryData>();
            purchasedPatches = new List<PartData>();

        }

        //====================================================================================================================//
        
    }
    
    

    
    
    

    

    


}
