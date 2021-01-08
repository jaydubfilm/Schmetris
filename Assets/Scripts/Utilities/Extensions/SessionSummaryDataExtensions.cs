using System.Collections.Generic;
using StarSalvager.Utilities.Analytics.Data;

namespace StarSalvager.Utilities.Extensions
{
    public static class SessionSummaryDataExtensions
    {
        public static SessionSummaryData Add(this SessionSummaryData sessionSummaryData, SessionSummaryData toAdd)
        {
            sessionSummaryData.totalTimeIn += toAdd.totalTimeIn;
            sessionSummaryData.timesKilled += toAdd.timesKilled;
            sessionSummaryData.TotalBumpersHit += toAdd.TotalBumpersHit;
            sessionSummaryData.totalDamageReceived += toAdd.totalDamageReceived;


            if (sessionSummaryData.BitSummaryData == null)
                sessionSummaryData.BitSummaryData = new List<BitSummaryData>();

            foreach (var bitSummary in toAdd.BitSummaryData)
            {
                var index = sessionSummaryData.BitSummaryData.FindIndex(x => x.type == bitSummary.type);
                if (index < 0)
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

            if (sessionSummaryData.ComponentSummaryData == null)
                sessionSummaryData.ComponentSummaryData = new List<ComponentSummaryData>();

            /*foreach (var componentSummary in toAdd.ComponentSummaryData)
            {
                var index = sessionSummaryData.ComponentSummaryData.FindIndex(x => x.type == componentSummary.type);
                if (index < 0)
                    sessionSummaryData.ComponentSummaryData.Add(componentSummary);
                else
                {
                    var temp = sessionSummaryData.ComponentSummaryData[index];

                    temp.collected += componentSummary.collected;
                    temp.diconnected += componentSummary.diconnected;

                    sessionSummaryData.ComponentSummaryData[index] = temp;
                }
            }*/

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