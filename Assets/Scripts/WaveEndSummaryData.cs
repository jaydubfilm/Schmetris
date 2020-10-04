using System.Collections.Generic;

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
        var outStringList = new List<string>
        {
            $"<b>Bonus Shapes Matched:</b> {numBonusShapesMatched}/{numTotalBonusShapesSpawned}",
            $"<b>Gears Gained:</b> {numGearsGained}",
        };
        //string returnString = string.Empty;

        //returnString += "Bonus Shapes Matched: " + numBonusShapesMatched + "/" + numTotalBonusShapesSpawned + "\n";
        //returnString += "Gears Gained: " + numGearsGained + "\n";

        if (numTotalEnemiesSpawned > 0)
        {
            outStringList.Add("<b>Enemies Killed:</b> ");
            //returnString += "Enemies Killed: " + "\n";
            foreach (var keyValuePair in dictTotalEnemiesSpawned)
            {
                //out default value for int should be 0
                dictEnemiesKilled.TryGetValue(keyValuePair.Key, out var amount);

                outStringList.Add($"\t{keyValuePair.Key}: {amount}/{keyValuePair.Value}");
                
                //if (dictEnemiesKilled.ContainsKey(value.Key))
                //{
                //    returnString += value.Key + " Killed: " + dictEnemiesKilled[value.Key] + "/" + value.Value + "\n";
                //}
                //else
                //{
                //    returnString += value.Key + " Killed: " + "0/" + value.Value + "\n";
                //}
            }
        }

        if (numLevelsGained > 0)
        {
            outStringList.Add($"<b>Level ups:</b> {numLevelsGained}");
        }

        if (blueprintsUnlockedStrings.Count > 0)
        {
            outStringList.Add($"<b>Blueprints Unlocked:</b> {string.Join(", ", blueprintsUnlockedStrings)}");
            /*returnString += "Blueprints Unlocked: ";
            for (int i = 0; i < blueprintsUnlockedStrings.Count; i++)
            {
                if (i > 0)
                {
                    returnString += ", ";
                }
                returnString += blueprintsUnlockedStrings[i];
            }
            returnString += "\n";*/
        }

        if (missionCompletedStrings.Count > 0)
        {
            outStringList.Add($"<b>Missions Completed:</b> {string.Join(", ", missionCompletedStrings)}");
            /*returnString += "Missions Completed: ";
            for (int i = 0; i < missionCompletedStrings.Count; i++)
            {
                if (i > 0)
                {
                    returnString += ", ";
                }
                returnString += missionCompletedStrings[i];
            }
            returnString += "\n";*/
        }

        if (missionUnlockedStrings.Count > 0)
        {
            outStringList.Add($"<b>Missions Unlocked:</b> {string.Join(", ", missionUnlockedStrings)}");
            /*returnString += "Missions Unlocked: ";
            for (int i = 0; i < missionUnlockedStrings.Count; i++)
            {
                if (i > 0)
                {
                    returnString += ", ";
                }
                returnString += missionUnlockedStrings[i];
                Debug.Log(missionUnlockedStrings[i]);
            }
            returnString += "\n";*/
        }

        return string.Join("\n", outStringList);
    }
}
