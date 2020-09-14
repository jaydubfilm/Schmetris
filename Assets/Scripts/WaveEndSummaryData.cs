using StarSalvager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEndSummaryData
{
    public int numBonusShapesMatched = 0;
    public int numTotalBonusShapesSpawned = 0;
    public int numEnemiesKilled = 0;
    public int numTotalEnemiesSpawned = 0;
    public Dictionary<string, int> dictEnemiesKilled = new Dictionary<string, int>();
    public Dictionary<string, int> dictTotalEnemiesSpawned = new Dictionary<string, int>();
    public int numGearsGained = 0;
    public int numLevelsGained = 0;
    public int numBlueprintsUnlocked = 0;
    public List<string> blueprintsUnlockedStrings = new List<string>();

    public string GetWaveEndSummaryDataString()
    {
        string returnString = string.Empty;

        returnString += "Bonus Shapes Matched: " + numBonusShapesMatched + "/" + numTotalBonusShapesSpawned + "\n";
        returnString += "Gears Gained: " + numGearsGained + "\n";

        if (numTotalEnemiesSpawned > 0)
        {
            returnString += "Enemies Killed: " + "\n";
            foreach (KeyValuePair<string, int> value in dictTotalEnemiesSpawned)
            {
                if (dictEnemiesKilled.ContainsKey(value.Key))
                {
                    returnString += value.Key + " Killed: " + dictEnemiesKilled[value.Key] + "/" + value.Value + "\n";
                }
                else
                {
                    returnString += value.Key + " Killed: " + "0/" + value.Value + "\n";
                }
            }
        }

        if (numLevelsGained > 0)
        {
            returnString += "Level ups: " + numLevelsGained + "\n";
        }

        if (numBlueprintsUnlocked > 0)
        {
            returnString += "Blueprints Unlocked: " + "\n";
            for (int i = 0; i < blueprintsUnlockedStrings.Count; i++)
            {
                returnString += blueprintsUnlockedStrings[i] + "\n";
            }
        }
        
        return returnString;
    }
}
