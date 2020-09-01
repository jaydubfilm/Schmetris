using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.Data
{
    [Serializable]
    public struct SessionSummaryData
    {
        [HideInInspector] public string Title;
        public string Date => date.ToString("ddd, MMM d, yyyy");
        private DateTime date;

        [Title("$Title",  "$Date")] [SerializeField, DisplayAsString]
        public float totalTimeIn;

        [SerializeField, DisplayAsString] public int timesKilled;

        [SerializeField, DisplayAsString] public int TotalBumpersHit;

        [SerializeField, DisplayAsString] public float totalDamageReceived;


        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<BitSummaryData> BitSummaryData;

        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<ComponentSummaryData> ComponentSummaryData;

        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<EnemySummaryData> enemiesKilledData;

        public SessionSummaryData(string Title, SessionData sessionData)
        {
            this.Title = Title;
            this.date = sessionData.date;
            
            var waves = sessionData.waves;

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
                    if (index < 0)
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
                    if (index < 0)
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
                    if (index < 0)
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

        public SessionSummaryData(string Title, IReadOnlyList<SessionData> sessionDatas)
        {
            var SessionData = new SessionSummaryData
            {
                Title = Title,
                date = sessionDatas[sessionDatas.Count - 1].date
            };

            SessionData = sessionDatas.Aggregate(SessionData,
                (current, sessionData) => current.Add(sessionData.GetSessionSummary()));

            this = SessionData;
        }

        public SessionSummaryData(string Title, IEnumerable<List<SessionData>> playerSessionsValues)
        {
            var sessionSummaryData = new SessionSummaryData
            {
                Title = Title,
                date = DateTime.UtcNow
            };

            foreach (var sessionsValues in playerSessionsValues)
            {
                foreach (SessionData sessionData in sessionsValues)
                {
                    sessionSummaryData = sessionSummaryData.Add(sessionData.GetSessionSummary());
                }
            }

            

            this = sessionSummaryData;
        }
    }

}