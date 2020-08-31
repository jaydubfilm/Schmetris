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

namespace StarSalvager.Utilities.Analytics
{
    [Serializable]
    public struct SessionData
    {
        public string PlayerID;
        public DateTime date;
        public List<WaveData> waves;

        public SessionSummaryData GetSessionSummary()
        {
            return new SessionSummaryData("Session Summary", waves);
        }
    }
    
    [Serializable]
    public struct SessionSummaryData
    {
        [HideInInspector]
        public string Title;
        
        [Title("$Title")]
        [SerializeField, DisplayAsString]
        public float totalTimeIn;
        
        [SerializeField, DisplayAsString]
        public int timesKilled;

        [SerializeField, DisplayAsString]
        public int TotalBumpersHit;

        [SerializeField, DisplayAsString]
        public float totalDamageReceived;

        
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<BitSummaryData> BitSummaryData;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<ComponentSummaryData> ComponentSummaryData;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<EnemySummaryData> enemiesKilledData;

        public SessionSummaryData(string Title, IReadOnlyCollection<WaveData> waves)
        {
            this.Title = Title;
            
            totalTimeIn = waves.Sum(x => x.timeIn);
            timesKilled = waves.Count(x => x.playerWasKilled);
            
            TotalBumpersHit = waves.Sum(x => x.bumpersHit);

            totalDamageReceived = waves.Sum(x => x.totalDamageReceived);


            BitSummaryData = new List<BitSummaryData>();
            ComponentSummaryData = new List<ComponentSummaryData>();
            enemiesKilledData = new List<EnemySummaryData>();

            foreach (var waveData in waves)
            {
                foreach (var bitSummary in waveData.BitSummaryData)
                {
                    var index = BitSummaryData.FindIndex(x => x.type == bitSummary.type);
                    if(index < 0)
                        BitSummaryData.Add(bitSummary);
                    else
                    {
                        var temp = BitSummaryData[index];

                        temp.collected += bitSummary.collected;
                        temp.diconnected += bitSummary.diconnected;
                        temp.liquidProcessed += bitSummary.liquidProcessed;

                        BitSummaryData[index] = temp;
                    }
                }
                
                foreach (var componentSummary in waveData.ComponentSummaryData)
                {
                    var index = ComponentSummaryData.FindIndex(x => x.type == componentSummary.type);
                    if(index < 0)
                        ComponentSummaryData.Add(componentSummary);
                    else
                    {
                        var temp = ComponentSummaryData[index];

                        temp.collected += componentSummary.collected;
                        temp.diconnected += componentSummary.diconnected;
                        
                        ComponentSummaryData[index] = temp;
                    }
                }
                
                foreach (var enemySummary in waveData.enemiesKilledData)
                {
                    var index = enemiesKilledData.FindIndex(x => x.id == enemySummary.id);
                    if(index < 0)
                        enemiesKilledData.Add(enemySummary);
                    else
                    {
                        var temp = enemiesKilledData[index];

                        temp.killed += enemySummary.killed;
                        
                        enemiesKilledData[index] = temp;
                    }
                }
            }

        }
        
        public SessionSummaryData(string Title, IEnumerable<SessionData> sessionDatas)
        {
            var SessionData = new SessionSummaryData
            {
                Title = Title
            };

            SessionData = sessionDatas.Aggregate(SessionData,
                (current, sessionData) => current.Add(sessionData.GetSessionSummary()));

            this = SessionData;
        }

    }

    public static class SessionSummaryDataExtensions
    {
        public static SessionSummaryData Add(this SessionSummaryData sessionSummaryData, SessionSummaryData toAdd)
        {
            sessionSummaryData.totalTimeIn += toAdd.totalTimeIn;
            sessionSummaryData.timesKilled += toAdd.timesKilled;
            sessionSummaryData.TotalBumpersHit += toAdd.TotalBumpersHit;
            sessionSummaryData.totalDamageReceived += toAdd.totalDamageReceived;
            
            
            if(sessionSummaryData.BitSummaryData == null)
                sessionSummaryData.BitSummaryData = new List<BitSummaryData>();
            
            foreach (var bitSummary in toAdd.BitSummaryData)
            {
                var index = sessionSummaryData.BitSummaryData.FindIndex(x => x.type == bitSummary.type);
                if(index < 0)
                    sessionSummaryData.BitSummaryData.Add(bitSummary);
                else
                {
                    var temp = sessionSummaryData.BitSummaryData[index];

                    temp.collected += bitSummary.collected;
                    temp.diconnected += bitSummary.diconnected;
                    temp.liquidProcessed += bitSummary.liquidProcessed;

                    sessionSummaryData.BitSummaryData[index] = temp;
                }
            }
            
            if(sessionSummaryData.ComponentSummaryData == null)
                sessionSummaryData.ComponentSummaryData = new List<ComponentSummaryData>();
                
            foreach (var componentSummary in toAdd.ComponentSummaryData)
            {
                var index = sessionSummaryData.ComponentSummaryData.FindIndex(x => x.type == componentSummary.type);
                if(index < 0)
                    sessionSummaryData.ComponentSummaryData.Add(componentSummary);
                else
                {
                    var temp = sessionSummaryData.ComponentSummaryData[index];

                    temp.collected += componentSummary.collected;
                    temp.diconnected += componentSummary.diconnected;
                        
                    sessionSummaryData.ComponentSummaryData[index] = temp;
                }
            }
            
            if(sessionSummaryData.enemiesKilledData == null)
                sessionSummaryData.enemiesKilledData = new List<EnemySummaryData>();
                
            foreach (var enemySummary in toAdd.enemiesKilledData)
            {
                var index = sessionSummaryData.enemiesKilledData.FindIndex(x => x.id == enemySummary.id);
                if(index < 0)
                    sessionSummaryData.enemiesKilledData.Add(enemySummary);
                else
                {
                    var temp = sessionSummaryData.enemiesKilledData[index];

                    temp.killed += enemySummary.killed;
                        
                    sessionSummaryData.enemiesKilledData[index] = temp;
                }
            }

            return sessionSummaryData;
        }
    }
    
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
        public List<BlockData> botAtStart;
        [HorizontalGroup("Row1")]
        public List<BlockData> botAtEnd;

        [TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<BitSummaryData> BitSummaryData;
        
        [TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<ComponentSummaryData> ComponentSummaryData;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<EnemySummaryData> enemiesKilledData;
        //TODO Need to add value for combos

        public WaveData(List<BlockData> botAtStart, int sectorNumber, int waveNumber)
        {
            this.botAtStart = botAtStart;
            this.sectorNumber = sectorNumber;
            this.waveNumber = waveNumber;

            bumpersHit = 0;
            totalDamageReceived = 0;
            timeIn = 0;
            
            playerWasKilled = false;
            
            date = DateTime.UtcNow;

            botAtEnd = new List<BlockData>();
            BitSummaryData = new List<BitSummaryData>();
            ComponentSummaryData = new List<ComponentSummaryData>();
            enemiesKilledData = new List<EnemySummaryData>();
        }
        
    }

    [Serializable]
    public struct BitSummaryData
    {
        //TODO Need to get the sprite image here
        [HideInInspector, HideInTables]
        public BIT_TYPE type;

        [DisplayAsString]
        public int liquidProcessed;
        [DisplayAsString]
        public int collected;
        [DisplayAsString]
        public int diconnected;
        
        #if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40), TableColumnWidth(50, Resizable = false)] 
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return Object.FindObjectOfType<FactoryManager>().BitProfileData.GetProfile(type).GetSprite(0);
        }
        
        
        #endif
    }

    [Serializable]
    public struct ComponentSummaryData
    {
        [HideInInspector, HideInTables]
        public COMPONENT_TYPE type;
        [DisplayAsString]
        public int collected;
        [DisplayAsString]
        public int diconnected;
        
#if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40), TableColumnWidth(50, Resizable = false)] 
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return Object.FindObjectOfType<FactoryManager>().ComponentProfile.GetProfile(type).GetSprite(0);
        }
        
        
#endif
    }

    [Serializable]
    public struct EnemySummaryData
    {
        [HideInTables]
        public string id;
        [DisplayAsString]
        public int killed;
        
#if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40), TableColumnWidth(50, Resizable = false)] 
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyProfileData(id).Sprite;
        }
        
        
#endif
    }
}
