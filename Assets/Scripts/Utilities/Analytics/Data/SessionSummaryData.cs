using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Analytics.Data;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.SessionTracking.Data
{
    [Serializable]
    public struct SessionSummaryData
    {
        //Properties
        //====================================================================================================================//
        
        [HideInInspector] public string Title;
        public string Date => date.ToString("ddd, MMM d, yyyy");
        private DateTime date;

        [Title("$Title",  "$Date")] [SerializeField, DisplayAsString]
        public float totalTimeIn;

        [SerializeField, DisplayAsString] public int timesKilled;

        [SerializeField, DisplayAsString] public int totalBumpersHit;

        [SerializeField, DisplayAsString] public float totalDamageReceived;


        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<BitSummaryData> bitSummaryData;

        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<EnemySummaryData> enemiesKilledData;

        //====================================================================================================================//
        
        public SessionSummaryData(string Title, SessionData sessionData)
        {
            this.Title = Title;
            date = sessionData.date;
            
            var waves = sessionData.waves;

            totalTimeIn = waves.Sum(x => x.timeIn);
            timesKilled = waves.Count(x => x.playerWasKilled);

            totalBumpersHit = waves.Sum(x => x.bumpersHit);

            totalDamageReceived = waves.Sum(x => x.totalDamageReceived);


            bitSummaryData = new List<BitSummaryData>();
            enemiesKilledData = new List<EnemySummaryData>();

            foreach (var waveData in waves)
            {
                foreach (var bitSummary in waveData.BitSummaryData)
                {
                    var bitData = bitSummary.bitData;
                    
                    var index = bitSummaryData
                        .FindIndex(x => x.bitData.Type == bitData.Type && x.bitData.Level == bitData.Level);
                    if (index < 0)
                        bitSummaryData.Add(bitSummary);
                    else
                    {
                        var temp = bitSummaryData[index];

                        temp.collected += bitSummary.collected;
                        temp.disconnected += bitSummary.disconnected;

                        bitSummaryData[index] = temp;
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

        public SessionSummaryData(in string title, in IReadOnlyList<SessionData> sessionDatas)
        {
            var sessionSummaryData = new SessionSummaryData
            {
                Title = title,
                date = sessionDatas[sessionDatas.Count - 1].date
            };

            sessionSummaryData = sessionDatas.Aggregate(sessionSummaryData,
                (current, sessionData) => current.Add(sessionData.GetSessionSummary()));

            this = sessionSummaryData;
        }

        public SessionSummaryData(in string title, in IEnumerable<List<SessionData>> playerSessionsValues)
        {
            var sessionSummaryData = new SessionSummaryData
            {
                Title = title,
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