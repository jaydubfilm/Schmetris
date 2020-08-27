using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics
{
    public class SessionDataProcessor : Singleton<SessionDataProcessor>
    {
        private SessionData _currentSession;

        private WaveData? _currentWave;

        //====================================================================================================================//

        private void Start()
        {
            _currentSession = new SessionData
            {
                //FIXME This needs to be generate somewhere
                id = "0",
                date = DateTime.UtcNow,
                waves = new List<WaveData>()
                
            };
        }

        private void OnApplicationQuit()
        {
            ExportSession();
        }

        //====================================================================================================================//
        
        public void StartNewWave(int sector, int wave, IEnumerable<BlockData> initialBot)
        {
            if (_currentWave.HasValue)
            {
                //TODO Need to end the existing wave
                EndActiveWave();
            }
            
            _currentWave = new WaveData
            {
                date = DateTime.UtcNow,
                sectorNumber = sector,
                waveNumber = wave,
                botAtStart = new List<BlockData>(initialBot)
                
            };
        }

        public void EndActiveWave()
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;

            wave.timeIn = (float)(DateTime.UtcNow - wave.date).TotalSeconds;
            
            
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
            
            if(wave.liquidProcessed == null)
                wave.liquidProcessed = new Dictionary<BIT_TYPE, int>();

            if (!wave.liquidProcessed.ContainsKey(type))
                wave.liquidProcessed.Add(type, amount);
            else
                wave.liquidProcessed[type] += amount;

            _currentWave = wave;
        }
        
        //TODO May want to consider including the levels with this value as well
        public void BitCollected(BIT_TYPE type)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.bitsCollected == null)
                wave.bitsCollected = new Dictionary<BIT_TYPE, int>();

            if (!wave.bitsCollected.ContainsKey(type))
                wave.bitsCollected.Add(type, 1);
            else
                wave.bitsCollected[type]++;

            _currentWave = wave;
        }
        
        public void BitDetached(BIT_TYPE type)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.bitsDisconnected == null)
                wave.bitsDisconnected = new Dictionary<BIT_TYPE, int>();

            if (!wave.bitsDisconnected.ContainsKey(type))
                wave.bitsDisconnected.Add(type, 1);
            else
                wave.bitsDisconnected[type]++;
            
            _currentWave = wave;
        }

        public void ComponentCollected(COMPONENT_TYPE type)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.componentsCollected == null)
                wave.componentsCollected = new Dictionary<COMPONENT_TYPE, int>();

            if (!wave.componentsCollected.ContainsKey(type))
                wave.componentsCollected.Add(type, 1);
            else
                wave.componentsCollected[type]++;
            
            _currentWave = wave;
        }

        public void EnemyKilled(string enemyId)
        {
            if (!_currentWave.HasValue)
                return;

            var wave = _currentWave.Value;
            
            if(wave.enemiesKilled == null)
                wave.enemiesKilled = new Dictionary<string, int>();

            if (!wave.enemiesKilled.ContainsKey(enemyId))
                wave.enemiesKilled.Add(enemyId, 1);
            else
                wave.enemiesKilled[enemyId]++;
            
            _currentWave = wave;
        }

        //====================================================================================================================//

        //TODO Move this to the Files location
        public void ExportSession()
        {
            if (_currentSession.waves.Count == 0)
                return;
            
            var path = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "RemoteData", $"Test_Session_{_currentSession.id}.txt");

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_currentSession, Formatting.Indented);
            
            File.WriteAllText(path, json);
        }
        
        
        //====================================================================================================================//
        
    }
}
