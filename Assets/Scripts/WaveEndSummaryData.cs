using StarSalvager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEndSummaryData
{
    public string waveEndTitle = string.Empty;
    public int numBonusShapesMatched = 0;
    public int numTotalBonusShapesSpawned = 0;
    public int numEnemiesKilled = 0;
    public int numTotalEnemiesSpawned = 0;
    public Dictionary<string, int> dictEnemiesKilled = new Dictionary<string, int>();
    public Dictionary<string, int> dictTotalEnemiesSpawned = new Dictionary<string, int>();
    public int numGearsGained = 0;
    public int numLevelsGained = 0;
    public List<string> blueprintsUnlockedStrings = new List<string>();
    public List<string> missionCompletedStrings = new List<string>();
    public List<string> missionUnlockedStrings = new List<string>();

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

        if (blueprintsUnlockedStrings.Count > 0)
        {
            returnString += "Blueprints Unlocked: ";
            for (int i = 0; i < blueprintsUnlockedStrings.Count; i++)
            {
                if (i > 0)
                {
                    returnString += ", ";
                }
                returnString += blueprintsUnlockedStrings[i];
            }
            returnString += "\n";
        }

        if (missionCompletedStrings.Count > 0)
        {
            returnString += "Missions Completed: ";
            for (int i = 0; i < missionCompletedStrings.Count; i++)
            {
                if (i > 0)
                {
                    returnString += ", ";
                }
                returnString += missionCompletedStrings[i];
            }
            returnString += "\n";
        }

        if (missionUnlockedStrings.Count > 0)
        {
            returnString += "Missions Unlocked: ";
            for (int i = 0; i < missionUnlockedStrings.Count; i++)
            {
                if (i > 0)
                {
                    returnString += ", ";
                }
                returnString += missionUnlockedStrings[i];
                Debug.Log(missionUnlockedStrings[i]);
            }
            returnString += "\n";
        }

        return returnString;
    }
}
