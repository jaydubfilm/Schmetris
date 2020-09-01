using System;
using System.Collections.Generic;
using StarSalvager.Utilities.Analytics.Data;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics
{
    public class SessionDataProcessor : Singleton<SessionDataProcessor>
    {
        private SessionData _currentSession;
        private int CurrentSession;

        private WaveData? _currentWave;

        private string playerID
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

        //====================================================================================================================//

        private void Start()
        {
            CurrentSession = 0;

            _currentSession = new SessionData
            {
                PlayerID = playerID,
                date = DateTime.UtcNow,
                waves = new List<WaveData>()
                
            };
        }

        private void OnApplicationQuit()
        {
            Files.ExportSessionData(playerID, _currentSession);
        }

        //====================================================================================================================//
        
        public void StartNewWave(int sector, int wave, IEnumerable<BlockData> initialBot)
        {
            if (_currentWave.HasValue)
            {
                //TODO Need to end the existing wave
                EndActiveWave();
            }

            var botAtStart = new List<BlockData>(initialBot);
            _currentWave = new WaveData(botAtStart, sector, wave);
        }

        public void EndActiveWave()
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.timeIn = (float)Math.Round((DateTime.UtcNow - wave.date).TotalSeconds, 2);
            
            
            _currentSession.waves.Add(wave);
            _currentWave = null;
        }

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

        public void ReceivedDamage(float damage)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.totalDamageReceived += damage;

            _currentWave = wave;
        }

        public void SetEndingLayout(IEnumerable<BlockData> botLayout)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.botAtEnd = new List<BlockData>(botLayout);
            _currentWave = wave;
        }

        public void LiquidProcessed(BIT_TYPE type, int amount)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.BitSummaryData == null)
                wave.BitSummaryData = new List<BitSummaryData>();

            var summaryIndex = wave.BitSummaryData.FindIndex(x => x.type == type);
            
            if(summaryIndex < 0)
                wave.BitSummaryData.Add(new BitSummaryData
                {
                    type = type,
                    liquidProcessed = amount
                });
            else
            {
                var tempData = wave.BitSummaryData[summaryIndex];
                tempData.liquidProcessed += amount;

                wave.BitSummaryData[summaryIndex] = tempData;
            }

                

            _currentWave = wave;
        }
        
        //TODO May want to consider including the levels with this value as well
        public void BitCollected(BIT_TYPE type)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            /*if(wave.bitsCollected == null)
                wave.bitsCollected = new Dictionary<BIT_TYPE, int>();

            if (!wave.bitsCollected.ContainsKey(type))
                wave.bitsCollected.Add(type, 1);
            else
                wave.bitsCollected[type]++;*/
            
            if(wave.BitSummaryData == null)
                wave.BitSummaryData = new List<BitSummaryData>();

            var summaryIndex = wave.BitSummaryData.FindIndex(x => x.type == type);
            
            if(summaryIndex < 0)
                wave.BitSummaryData.Add(new BitSummaryData
                {
                    type = type,
                    collected = 1
                });
            else
            {
                var tempData = wave.BitSummaryData[summaryIndex];
                tempData.collected++;

                wave.BitSummaryData[summaryIndex] = tempData;
            }

            _currentWave = wave;
        }
        
        public void BitDetached(BIT_TYPE type)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            /*if(wave.bitsDisconnected == null)
                wave.bitsDisconnected = new Dictionary<BIT_TYPE, int>();

            if (!wave.bitsDisconnected.ContainsKey(type))
                wave.bitsDisconnected.Add(type, 1);
            else
                wave.bitsDisconnected[type]++;*/
            
            if(wave.BitSummaryData == null)
                wave.BitSummaryData = new List<BitSummaryData>();

            var summaryIndex = wave.BitSummaryData.FindIndex(x => x.type == type);
            
            if(summaryIndex < 0)
                wave.BitSummaryData.Add(new BitSummaryData
                {
                    type = type,
                    diconnected = 1
                });
            else
            {
                var tempData = wave.BitSummaryData[summaryIndex];
                tempData.diconnected++;

                wave.BitSummaryData[summaryIndex] = tempData;
            }
            
            _currentWave = wave;
        }

        public void ComponentCollected(COMPONENT_TYPE type)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            /*if(wave.componentsCollected == null)
                wave.componentsCollected = new Dictionary<COMPONENT_TYPE, int>();

            if (!wave.componentsCollected.ContainsKey(type))
                wave.componentsCollected.Add(type, 1);
            else
                wave.componentsCollected[type]++;*/
            if(wave.ComponentSummaryData == null)
                wave.ComponentSummaryData = new List<ComponentSummaryData>();

            var summaryIndex = wave.ComponentSummaryData.FindIndex(x => x.type == type);
            
            if(summaryIndex < 0)
                wave.ComponentSummaryData.Add(new ComponentSummaryData
                {
                    type = type,
                    collected = 1
                });
            else
            {
                var tempData = wave.ComponentSummaryData[summaryIndex];
                tempData.collected++;

                wave.ComponentSummaryData[summaryIndex] = tempData;
            }
            
            _currentWave = wave;
        }

        public void EnemyKilled(string enemyId)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            /*if(wave.enemiesKilled == null)
                wave.enemiesKilled = new Dictionary<string, int>();

            if (!wave.enemiesKilled.ContainsKey(enemyId))
                wave.enemiesKilled.Add(enemyId, 1);
            else
                wave.enemiesKilled[enemyId]++;*/
            if(wave.enemiesKilledData == null)
                wave.enemiesKilledData = new List<EnemySummaryData>();

            //FIXME I hate the long string comparisons happening here
            var summaryIndex = wave.enemiesKilledData.FindIndex(x => x.id == enemyId);
            
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

        //====================================================================================================================//
        
        
        
        //====================================================================================================================//
        
    }
}
