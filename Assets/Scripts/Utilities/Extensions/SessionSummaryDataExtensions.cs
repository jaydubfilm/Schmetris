using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Utilities.Analytics.Data;

using StarSalvager.Utilities.Analytics.SessionTracking.Data;

namespace StarSalvager.Utilities.Extensions
{
    internal static class SessionSummaryDataExtensions
    {
        public static SessionSummaryData Add(this SessionSummaryData sessionSummaryData, SessionSummaryData toAdd)
        {

            //--------------------------------------------------------------------------------------------------------//

            sessionSummaryData.totalTimeIn += toAdd.totalTimeIn;
            sessionSummaryData.timesKilled += toAdd.timesKilled;
            sessionSummaryData.totalBumpersHit += toAdd.totalBumpersHit;
            sessionSummaryData.totalDamageReceived += toAdd.totalDamageReceived;

            //--------------------------------------------------------------------------------------------------------//

            sessionSummaryData.totalXpEarned += toAdd.totalXpEarned;
            sessionSummaryData.totalGearsCollected += toAdd.totalGearsCollected;
            sessionSummaryData.totalSilverEarned += toAdd.totalSilverEarned;

            sessionSummaryData.totalGearsSpent += toAdd.totalGearsSpent;
            sessionSummaryData.totalSilverSpent += toAdd.totalSilverSpent;

            //Bit Data Adding
            //--------------------------------------------------------------------------------------------------------//

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
                    temp.collected += bitSummary.spawned;
                    temp.disconnected += bitSummary.disconnected;

                    sessionSummaryData.bitSummaryData[index] = temp;
                }
            }

            //Combo Data Adding
            //--------------------------------------------------------------------------------------------------------//
            if (sessionSummaryData.comboSummaryData == null)
                sessionSummaryData.comboSummaryData = new List<ComboSummaryData>();

            foreach (var comboSummary in toAdd.comboSummaryData)
            {
                var bitData = comboSummary.bitData;

                var index = sessionSummaryData.comboSummaryData
                    .FindIndex(x =>
                        x.bitData.Type == bitData.Type &&
                        x.bitData.Level == bitData.Level &&
                        x.comboType == comboSummary.comboType);

                if (index < 0)
                    sessionSummaryData.comboSummaryData.Add(comboSummary);
                else
                {
                    var temp = sessionSummaryData.comboSummaryData[index];

                    temp.created += comboSummary.created;

                    sessionSummaryData.comboSummaryData[index] = temp;
                }
            }

            //Enemy Data Adding
            //--------------------------------------------------------------------------------------------------------//

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
                    temp.spawned += enemySummary.spawned;
                    
                    sessionSummaryData.enemiesKilledData[index] = temp;
                }
            }

            //Part Selection Aggregation
            //--------------------------------------------------------------------------------------------------------//
            if (sessionSummaryData.partSelectionSummaries == null)
                sessionSummaryData.partSelectionSummaries = new List<SessionSummaryData.PartSelectionSummary>();
            
            foreach (var selectionSummary in toAdd.partSelectionSummaries)
            {
                var index = sessionSummaryData.partSelectionSummaries
                    .FindIndex(x => x.partType == selectionSummary.partType);

                if (index < 0)
                {
                    sessionSummaryData.partSelectionSummaries.Add(new SessionSummaryData.PartSelectionSummary
                    {
                        partType = selectionSummary.partType,
                        timesAppeared = selectionSummary.timesAppeared,
                        timesPicked = selectionSummary.timesPicked
                    });
                }
                else
                {
                    var data = sessionSummaryData.partSelectionSummaries[index];
                    data.timesAppeared += selectionSummary.timesAppeared;
                    data.timesPicked += selectionSummary.timesPicked;

                    sessionSummaryData.partSelectionSummaries[index] = data;
                }
            }

            
            //Part Discard Aggregation
            //--------------------------------------------------------------------------------------------------------//
            
            if (sessionSummaryData.partDiscardSummaries == null)
                sessionSummaryData.partDiscardSummaries = new List<SessionSummaryData.PartSelectionSummary>();
            
            foreach (var selectionSummary in toAdd.partDiscardSummaries)
            {
                var index = sessionSummaryData.partDiscardSummaries
                    .FindIndex(x => x.partType == selectionSummary.partType);

                if (index < 0)
                {
                    sessionSummaryData.partDiscardSummaries.Add(new SessionSummaryData.PartSelectionSummary
                    {
                        partType = selectionSummary.partType,
                        timesAppeared = selectionSummary.timesAppeared,
                        timesPicked = selectionSummary.timesPicked
                    });
                }
                else
                {
                    var data = sessionSummaryData.partDiscardSummaries[index];
                    data.timesAppeared += selectionSummary.timesAppeared;
                    data.timesPicked += selectionSummary.timesPicked;

                    sessionSummaryData.partDiscardSummaries[index] = data;
                }
            }


            //Patch Purchase Aggregation
            //--------------------------------------------------------------------------------------------------------//
            
            if (sessionSummaryData.PatchPurchaseSummaries == null)
                sessionSummaryData.PatchPurchaseSummaries = new List<SessionSummaryData.PatchPurchaseSummary>();
            
            foreach (var purchaseSummary in toAdd.PatchPurchaseSummaries)
            {
                var index = sessionSummaryData.PatchPurchaseSummaries
                    .FindIndex(x => x.partType == purchaseSummary.partType);

                if (index < 0)
                {
                    sessionSummaryData.PatchPurchaseSummaries.Add(new SessionSummaryData.PatchPurchaseSummary
                    {
                        partType = purchaseSummary.partType,
                        patchData = purchaseSummary.patchData,
                        amount = purchaseSummary.amount,
                    });
                }
                else
                {
                    var data = sessionSummaryData.PatchPurchaseSummaries[index];
                    data.amount += purchaseSummary.amount;

                    sessionSummaryData.PatchPurchaseSummaries[index] = data;
                }
            }
            
            var t = TimeSpan.FromSeconds( sessionSummaryData.totalTimeIn );
            sessionSummaryData.TimeString = $"{t.Hours:D2}h:{t.Minutes:D2}m:{t.Seconds:D2}s";

            return sessionSummaryData;
        }
    }
}