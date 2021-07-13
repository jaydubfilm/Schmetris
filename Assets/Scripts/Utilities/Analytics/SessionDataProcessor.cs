using System;
using System.Collections.Generic;
using StarSalvager.Utilities.Analytics.Data;
using StarSalvager.Utilities.Analytics.SessionTracking.Data;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Puzzle.Structs;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.SessionTracking
{
    //TODO Need to implement the total Game-data for a player (A save file)
    //FIXME Need to better implement the data recording, to not constantly be retrieving and overwriting the data
    //FIXME Try to make all calls happen to static functions, dislike having to call instance each time.
    public class SessionDataProcessor : Singleton<SessionDataProcessor>
    {
        //Properties
        //====================================================================================================================//
        
        public static readonly Version VERSION = new Version(2,0,0,1);
        private SessionData _currentSession;
        private int CurrentSession;

        private WaveData? _currentWave;

        private string PlayerID
        {
            get
            {
                _playerId = PlayerPrefs.GetString("PlayerID");
                
                if (!string.IsNullOrEmpty(_playerId))
                    return _playerId;
                
                _playerId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("PlayerID", _playerId);
                PlayerPrefs.Save();

                return _playerId;
            }
        }
        private string _playerId;

        //Unity Functions
        //====================================================================================================================//

        private void Start()
        {
            CurrentSession = 0;

            _currentSession = new SessionData
            {
                Version = VERSION,
                PlayerID = PlayerID,
                date = DateTime.UtcNow,
                waves = new List<WaveData>()
                
            };
        }

        //File Functions
        //====================================================================================================================//

        public static void ExportSessionData() => Files.ExportSessionData(Instance.PlayerID, Instance._currentSession);

        //Wave Functions
        //====================================================================================================================//
        
        public void StartNewWave(in int ring, in int wave, in IEnumerable<IBlockData> initialBot)
        {
            if (_currentWave.HasValue)
            {
                //Need to end the existing wave
                EndActiveWave();
            }

            var botAtStart = new List<IBlockData>(initialBot);
            _currentWave = new WaveData(botAtStart, ring, wave);


        }
        public void StartNewWreck(in Vector2Int coordinates)
        {
            if (_currentWave.HasValue)
            {
                //Need to end the existing wave
                EndActiveWave();
            }

            _currentWave = new WaveData(true, coordinates);
        }

        public void EndActiveWave()
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.timeIn = (float)System.Math.Round((DateTime.UtcNow - wave.date).TotalSeconds, 2);
            
            
            _currentSession.waves.Add(wave);
            _currentWave = null;
        }

        //Wave Data recording Functions
        //====================================================================================================================//

        public void PlayerKilled()
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.playerWasKilled = true;

            _currentWave = wave;
        }

        public void HitBumper()
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.bumpersHit++;
            _currentWave = wave;
        }

        public void ReceivedDamage(in float damage)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.totalDamageReceived += damage;

            _currentWave = wave;
        }

        public void SetEndingLayout(in IEnumerable<IBlockData> botLayout)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.botAtEnd = new List<IBlockData>(botLayout);
            _currentWave = wave;
        }

        //TODO May want to consider including the levels with this value as well
        public void RecordBitConnected(in BitData bitData)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            var tempBitData = bitData;

            if(wave.BitSummaryData == null) wave.BitSummaryData = new List<BitSummaryData>();

            var index = wave.BitSummaryData
                .FindIndex(x => x.bitData.Type == tempBitData.Type && x.bitData.Level == tempBitData.Level);
            
            if(index < 0)
                wave.BitSummaryData.Add(new BitSummaryData
                {
                    bitData = bitData,
                    collected = 1
                });
            else
            {
                var tempData = wave.BitSummaryData[index];
                tempData.collected++;

                wave.BitSummaryData[index] = tempData;
            }

            _currentWave = wave;
        }
        
        public void RecordBitSpawned(in BitData bitData)
        {
            if (!_currentWave.HasValue) return;

            var wave = _currentWave.Value;
            var tempBitData = bitData;

            if(wave.BitSummaryData == null) wave.BitSummaryData = new List<BitSummaryData>();

            var index = wave.BitSummaryData
                .FindIndex(x => x.bitData.Type == tempBitData.Type && x.bitData.Level == tempBitData.Level);
            
            if(index < 0)
                wave.BitSummaryData.Add(new BitSummaryData
                {
                    bitData = bitData,
                    spawned = 1
                });
            else
            {
                var tempData = wave.BitSummaryData[index];
                tempData.spawned++;

                wave.BitSummaryData[index] = tempData;
            }

            _currentWave = wave;
        }
        
        public void RecordBitDetached(in BitData bitData)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            var tempBitData = bitData;
            
            if(wave.BitSummaryData == null)
                wave.BitSummaryData = new List<BitSummaryData>();

            var index = wave.BitSummaryData
                .FindIndex(x => x.bitData.Type == tempBitData.Type && x.bitData.Level == tempBitData.Level);
            
            if(index < 0)
                wave.BitSummaryData.Add(new BitSummaryData
                {
                    bitData = bitData,
                    disconnected = 1
                });
            else
            {
                var tempData = wave.BitSummaryData[index];
                tempData.disconnected++;

                wave.BitSummaryData[index] = tempData;
            }
            
            _currentWave = wave;
        }

        //PlayerDataManager Piggy-backing functions
        //====================================================================================================================//
        public void EnemySpawned(in string enemyId)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.enemiesKilledData == null)
                wave.enemiesKilledData = new List<EnemySummaryData>();

            //FIXME I hate the long string comparisons happening here
            var id = enemyId;
            var summaryIndex = wave.enemiesKilledData.FindIndex(x => x.id == id);
            
            if(summaryIndex < 0)
                wave.enemiesKilledData.Add(new EnemySummaryData
                {
                    id = enemyId,
                    spawned = 1
                });
            else
            {
                var tempData = wave.enemiesKilledData[summaryIndex];
                tempData.spawned++;

                wave.enemiesKilledData[summaryIndex] = tempData;
            }
            
            _currentWave = wave;
        }
        public void EnemyKilled(in string enemyId)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.enemiesKilledData == null)
                wave.enemiesKilledData = new List<EnemySummaryData>();

            //FIXME I hate the long string comparisons happening here
            var id = enemyId;
            var summaryIndex = wave.enemiesKilledData.FindIndex(x => x.id == id);
            
            if(summaryIndex < 0)
                wave.enemiesKilledData.Add(new EnemySummaryData
                {
                    id = enemyId,
                    killed = 1
                });
            else
            {
                var tempData = wave.enemiesKilledData[summaryIndex];
                tempData.killed++;

                wave.enemiesKilledData[summaryIndex] = tempData;
            }
            
            _currentWave = wave;
        }
        public void RecordCombo(in ComboRecordData comboRecordData)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.comboSummaryData == null)
                wave.comboSummaryData = new List<ComboSummaryData>();

            var comboSummary = new ComboSummaryData(comboRecordData);

            var index = wave.comboSummaryData
                .FindIndex(x => x.Equals(comboSummary));
            
            if (index >= 0)
            {
                var data = wave.comboSummaryData[index];
                data.created++;
                wave.comboSummaryData[index] = data;
            }
            else
                wave.comboSummaryData.Add(comboSummary);

            _currentWave = wave;
        }

        public void RecordXPEarned(in int xp)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.xpEarned += xp;

            _currentWave = wave;
        }

        public void RecordSilverEarned(in int silver)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.silverEarned += silver;

            _currentWave = wave;
        }

        public void RecordGearsEarned(in int gears)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.gearsCollected += gears;

            _currentWave = wave;
        }
        
        public void RecordSilverSpent(in int silver)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.spentSilver += silver;

            _currentWave = wave;
        }

        public void RecordGearsSpent(in int gears)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.spentGears += gears;

            _currentWave = wave;
        }

        //====================================================================================================================//

        
        public void RecordPartSelection(in PART_TYPE selected, in PART_TYPE[] options)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.SelectedPart = new PartSelectionData(selected, options);

            _currentWave = wave;
        }

        public void RecordPartDiscarding(in PART_TYPE selected, in PART_TYPE[] options)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.DiscardedPart = new PartSelectionData(selected, options);

            _currentWave = wave;
        }

        public void RecordPatchPurchase(in PartData patchPurchase)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            var partTypeInt = patchPurchase.Type;
            
            var index = wave.purchasedPatches.FindIndex(x => x.Type == partTypeInt);
            if(index < 0)
                wave.purchasedPatches.Add(patchPurchase);
            else
                wave.purchasedPatches[index].Patches.AddRange(patchPurchase.Patches);
            
            _currentWave = wave;
        }

        public void RecordPartsInStorage(in IEnumerable<PartData> partsInStorage)
        {
            throw new NotImplementedException();
        }
        
    }
}
