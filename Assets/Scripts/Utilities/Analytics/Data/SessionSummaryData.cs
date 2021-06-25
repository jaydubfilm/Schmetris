using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Analytics.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.SessionTracking.Data
{
    [Serializable]
    public struct SessionSummaryData
    {
        [Serializable]
        public struct PartSelectionSummary
        {
            [DisplayAsString]
            public PART_TYPE partType;
            [DisplayAsString]
            public int timesPicked;
            [DisplayAsString]
            public int timesAppeared;
        }
        
        [Serializable]
        public struct PatchPurchaseSummary
        {
            [DisplayAsString]
            public PART_TYPE partType;

            [DisplayAsString]
            public PatchData patchData;
            
            [DisplayAsString]
            public int amount;
        }
        
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

        [SerializeField, DisplayAsString] 
        public int totalXpEarned;
        [SerializeField, DisplayAsString] 
        public int totalGearsCollected;
        [SerializeField, DisplayAsString] 
        public int totalSilverEarned;
        


        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<BitSummaryData> bitSummaryData;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<ComboSummaryData> comboSummaryData;
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<EnemySummaryData> enemiesKilledData;

        //====================================================================================================================//

        [Title("Wreck Data")]
        [Title("Chosen Parts", horizontalLine:false, bold:false)]
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<PartSelectionSummary> partSelectionSummaries;
        [Title("Discarded Parts", horizontalLine:false, bold:false)]
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<PartSelectionSummary> partDiscardSummaries;

        //====================================================================================================================//

        [SerializeField, DisplayAsString] 
        public int totalGearsSpent; 
        [SerializeField, DisplayAsString] 
        public int totalSilverSpent;
        
        [Title("Purchased Patches", horizontalLine:false, bold:false)]
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        public List<PatchPurchaseSummary> PatchPurchaseSummaries;

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


            //--------------------------------------------------------------------------------------------------------//

            totalXpEarned = waves.Sum(x => x.xpEarned);
            totalGearsCollected = waves.Sum(x => x.gearsCollected);
            totalSilverEarned = waves.Sum(x => x.silverEarned);

            totalGearsSpent = waves.Sum(x => x.spentGears);
            totalSilverSpent = waves.Sum(x => x.spentSilver);

            //--------------------------------------------------------------------------------------------------------//
            
            bitSummaryData = new List<BitSummaryData>();
            enemiesKilledData = new List<EnemySummaryData>();
            comboSummaryData = new List<ComboSummaryData>();

            partSelectionSummaries = new List<PartSelectionSummary>();
            partDiscardSummaries = new List<PartSelectionSummary>();
            
            PatchPurchaseSummaries = new List<PatchPurchaseSummary>();
            
            foreach (var waveData in waves)
            {

                //Bit Data aggregation
                //--------------------------------------------------------------------------------------------------------//
                
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

                //Combo Data aggregation
                //--------------------------------------------------------------------------------------------------------//
                
                foreach (var comboSummary in waveData.comboSummaryData)
                {
                    var bitData = comboSummary.bitData;
                    
                    var index = comboSummaryData
                        .FindIndex(x => 
                                        x.bitData.Type == bitData.Type &&
                                        x.bitData.Level == bitData.Level &&
                                        x.comboType == comboSummary.comboType);
                    
                    if (index < 0)
                        comboSummaryData.Add(comboSummary);
                    else
                    {
                        var temp = comboSummaryData[index];

                        temp.created += comboSummary.created;

                        comboSummaryData[index] = temp;
                    }
                }

                //Enemy Data aggregation
                //--------------------------------------------------------------------------------------------------------//
                
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

                //Part Selection Aggregation
                //--------------------------------------------------------------------------------------------------------//
                var partSelection = waveData.SelectedPart;
                foreach (var selectionOption in partSelection.Options)
                {
                    var index = partSelectionSummaries.FindIndex(x => x.partType == selectionOption);

                    if (index < 0)
                    {
                        partSelectionSummaries.Add(new PartSelectionSummary
                        {
                            partType = selectionOption,
                            timesAppeared = 1,
                            timesPicked = partSelection.Selected == selectionOption ? 1 : 0
                        });
                    }
                    else
                    {
                        var data = partSelectionSummaries[index];
                        data.timesAppeared++;

                        if (selectionOption == partSelection.Selected) data.timesPicked++;

                        partSelectionSummaries[index] = data;
                    }
                }

                //Part Discard Aggregation
                //--------------------------------------------------------------------------------------------------------//
                var partDiscard = waveData.DiscardedPart;
                foreach (var selectionOption in partDiscard.Options)
                {
                    var index = partDiscardSummaries.FindIndex(x => x.partType == selectionOption);

                    if (index < 0)
                    {
                        partDiscardSummaries.Add(new PartSelectionSummary
                        {
                            partType = selectionOption,
                            timesAppeared = 1,
                            timesPicked = partDiscard.Selected == selectionOption ? 1 : 0
                        });
                    }
                    else
                    {
                        var data = partDiscardSummaries[index];
                        data.timesAppeared++;

                        if (selectionOption == partDiscard.Selected)
                            data.timesPicked++;

                        partDiscardSummaries[index] = data;
                    }
                }

                //Patch Purchase Aggregation
                //--------------------------------------------------------------------------------------------------------//

                if (waveData.purchasedPatches.IsNullOrEmpty())
                    continue;
                
                foreach (var purchasedPatch in waveData.purchasedPatches)
                {
                    var partType = (PART_TYPE) purchasedPatch.Type;
                    foreach (var patchData in purchasedPatch.Patches)
                    {
                        var index = PatchPurchaseSummaries
                            .FindIndex(x => x.partType == partType &&
                                            x.patchData.Equals(patchData));

                        if (index < 0)
                        {
                            PatchPurchaseSummaries.Add(new PatchPurchaseSummary
                            {
                                partType = partType,
                                patchData = patchData,
                                amount = 1
                            });
                        }
                        else
                        {
                            var data = PatchPurchaseSummaries[index];
                            data.amount++;
                            PatchPurchaseSummaries[index] = data;
                        }
                    }
                    PatchPurchaseSummaries =
                        new List<PatchPurchaseSummary>(
                            PatchPurchaseSummaries.OrderBy(x => x.partType));
                    
                }

                //--------------------------------------------------------------------------------------------------------//
                
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