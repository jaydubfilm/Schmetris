using System.Collections.Generic;
using StarSalvager.Utilities.Analytics.Data;

using StarSalvager.Utilities.Analytics.SessionTracking.Data;

namespace StarSalvager.Utilities.Extensions
{
    public static class SessionSummaryDataExtensions
    {
        public static SessionSummaryData Add(this SessionSummaryData sessionSummaryData, SessionSummaryData toAdd)
        {
            sessionSummaryData.totalTimeIn += toAdd.totalTimeIn;
            sessionSummaryData.timesKilled += toAdd.timesKilled;
            sessionSummaryData.totalBumpersHit += toAdd.totalBumpersHit;
            sessionSummaryData.totalDamageReceived += toAdd.totalDamageReceived;


            if (sessionSummaryData.bitSummaryData == null)
                sessionSummaryData.bitSummaryData = new List<BitSummaryData>();

            foreach (var bitSummary in toAdd.bitSummaryData)
            {
                var bitData = bitSummary.bitData;
                var index = sessionSummaryData.bitSummaryData
                    .FindIndex(x => x.bitData.Type == bitData.Type && x.bitData.Level == bitData.Level);
                if (index < 0)
                    sessionSummaryData.bitSummaryData.Add(bitSummary);
                else
                {
                    var temp = sessionSummaryData.bitSummaryData[index];

                    temp.collected += bitSummary.collected;
                    temp.disconnected += bitSummary.disconnected;

                    sessionSummaryData.bitSummaryData[index] = temp;
                }
            }

            if (sessionSummaryData.enemiesKilledData == null)
                sessionSummaryData.enemiesKilledData = new List<EnemySummaryData>();

            foreach (var enemySummary in toAdd.enemiesKilledData)
            {
                var index = sessionSummaryData.enemiesKilledData.FindIndex(x => x.id == enemySummary.id);
                if (index < 0)
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
}