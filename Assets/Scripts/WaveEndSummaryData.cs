using System.Collections.Generic;
using System.Linq;
using StarSalvager;
using StarSalvager.Utilities;
using StarSalvager.Utilities.UI;
using UnityEngine;

public class WaveEndSummaryData
{
    public string WaveEndTitle;
    public int NumBonusShapesMatched;
    public int NumTotalBonusShapesSpawned;
    
    public int NumGearsGained;
    public int NumLevelsGained;
    
    
    private int _numEnemiesKilled;
    private int _numTotalEnemiesSpawned;
    private readonly Dictionary<string, int> _dictEnemiesKilled;
    private readonly Dictionary<string, int> _dictTotalEnemiesSpawned;

    private readonly List<string> _blueprintsUnlockedStrings;
    private readonly List<string> _missionCompletedStrings;
    private readonly List<string> _missionUnlockedStrings;

    private readonly Dictionary<BIT_TYPE, float> _resourcesConsumed;
    
    public int CompletedSector;
    public int CompletedWave;


    //====================================================================================================================//
    
    public WaveEndSummaryData()
    {
        WaveEndTitle = string.Empty;
        NumBonusShapesMatched = 0;
        NumTotalBonusShapesSpawned = 0;
        _numEnemiesKilled = 0;
        _numTotalEnemiesSpawned = 0;
        _dictEnemiesKilled = new Dictionary<string, int>();
        _dictTotalEnemiesSpawned = new Dictionary<string, int>();
        NumGearsGained = 0;
        NumLevelsGained = 0;
        _blueprintsUnlockedStrings = new List<string>();
        _missionCompletedStrings = new List<string>();
        _missionUnlockedStrings = new List<string>();
        _resourcesConsumed = new Dictionary<BIT_TYPE, float>();
    }

    public string GetWaveEndSummaryDataString()
    {
        var outStringList = new List<string>
        {
            $"{GetAsTitle("Gears Gained")} {NumGearsGained}",
        };

        if (NumTotalBonusShapesSpawned > 0)
            outStringList.Add($"{GetAsTitle("Bonus Shapes Matched")} {NumBonusShapesMatched}/{NumTotalBonusShapesSpawned}");

        if (_numTotalEnemiesSpawned > 0)
        {
            outStringList.Add($"{GetAsTitle("Enemies Killed")}");
            foreach (var keyValuePair in _dictTotalEnemiesSpawned)
            {
                //out default value for int should be 0
                _dictEnemiesKilled.TryGetValue(keyValuePair.Key, out var amount);

                outStringList.Add($"\t{keyValuePair.Key}: {amount}/{keyValuePair.Value}");
            }
        }

        if (NumLevelsGained > 0)
        {
            outStringList.Add($"{GetAsTitle("Level ups")} {NumLevelsGained}");
        }

        if (_blueprintsUnlockedStrings.Count > 0)
        {
            outStringList.Add($"{GetAsTitle("Blueprints Unlocked")} {string.Join(", ", _blueprintsUnlockedStrings)}");
        }

        if (_missionCompletedStrings.Count > 0)
        {
            outStringList.Add($"{GetAsTitle("Missions Completed")} {string.Join(", ", _missionCompletedStrings)}");
        }

        if (_missionUnlockedStrings.Count > 0)
        {
            outStringList.Add($"{GetAsTitle("Missions Unlocked")} {string.Join(", ", _missionUnlockedStrings)}");
        }

        if (_resourcesConsumed. Count > 0)
        {
            outStringList.Add($"{GetAsTitle("Resources Consumed")} ");

            var joinList = new List<string>();
            foreach (var keyValuePair in _resourcesConsumed)
            {
                var image = TMP_SpriteMap.MaterialIcons[keyValuePair.Key];
                
                joinList.Add($"\t{image}: {keyValuePair.Value:N0}");
            }
            outStringList.Add(string.Join(", ", joinList));
        }
        return string.Join("\n", outStringList);
    }

    private static string GetAsTitle(in string title)
    {
        return $"<b><color=#00FFBF>{title}:</color></b>";
    }

    //====================================================================================================================//

    public void AddGearsGained(int gearsAmount)
    {
        NumGearsGained += gearsAmount;
    }

    public void AddEnemyKilled(string enemyName)
    {
        if (!_dictEnemiesKilled.ContainsKey(enemyName))
        {
            _dictEnemiesKilled.Add(enemyName, 0);
        }
        
        _dictEnemiesKilled[enemyName]++;
        LevelManager.Instance.WaveEndSummaryData._numEnemiesKilled++;


    }
    public void AddEnemySpawned(string enemyName)
    {if (!_dictTotalEnemiesSpawned.ContainsKey(enemyName))
        {
            _dictTotalEnemiesSpawned.Add(enemyName, 0);
        }
        
        _dictTotalEnemiesSpawned[enemyName]++;
        LevelManager.Instance.WaveEndSummaryData._numTotalEnemiesSpawned++;}

    public void AddUnlockedBlueprint(string bluePrintName)
    {
        _blueprintsUnlockedStrings.Add(bluePrintName);
    }

    public void AddCompletedMission(string missionName)
    {
        _missionCompletedStrings.Add(missionName);
    }

    public void AddUnlockedMission(string missionName)
    {
        _missionUnlockedStrings.Add(missionName);
    }

    public void AddConsumedBit(BIT_TYPE type, float amount)
    {
        if (amount == 0f)
            return;
        
        if(!_resourcesConsumed.ContainsKey(type))
            _resourcesConsumed.Add(type, 0f);

        _resourcesConsumed[type] += amount;
    }

    //====================================================================================================================//

    public Dictionary<string, object> GetWaveEndSummaryAnalytics()
    {
        var enemiesKilledPercentage = 0.0f;
        if (_numTotalEnemiesSpawned > 0)
        {
            enemiesKilledPercentage = _numEnemiesKilled / (float)_numTotalEnemiesSpawned;
        }
        
        var bonusShapesPercentage = 0.0f;
        if (NumTotalBonusShapesSpawned > 0)
        {
            bonusShapesPercentage = NumBonusShapesMatched / (float)NumTotalBonusShapesSpawned;
        }
        
        return new Dictionary<string, object>
        {
            {AnalyticsManager.GearsGained, NumGearsGained },
            {AnalyticsManager.LevelsGained, NumLevelsGained },
            {AnalyticsManager.EnemiesKilled, _numEnemiesKilled },
            {AnalyticsManager.EnemiesKilledPercentage, enemiesKilledPercentage },
            {AnalyticsManager.BonusShapesMatched, NumBonusShapesMatched },
            {AnalyticsManager.BonusShapesMatchedPercentage, bonusShapesPercentage },
            {AnalyticsManager.BlueprintsUnlocked, _blueprintsUnlockedStrings.Count },
            {AnalyticsManager.MissionsCompleted, _missionCompletedStrings.Count },
            {AnalyticsManager.MissionsUnlocked, _missionUnlockedStrings.Count }
        };
    }
    
}
