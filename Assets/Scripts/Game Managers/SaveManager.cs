using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class SaveManager
{
    //Where to save game data
    const string saveDirectory = "/SaveData/";
    const string saveFile = "Game.json";
    string fullSavePath;

    //Loaded save data
    public bool hasSaveData = false;
    GameData saveData = new GameData();
    const int maxSaveFiles = 3;

    //Return data from save number
    public SaveData GetSave(int index)
    {
        if(!hasSaveData)
        {
            Init();
        }
        return saveData.saveFiles[index];
    }

    //Return layout from save number
    public SaveData GetLayout(int index)
    {
        if (!hasSaveData)
        {
            Init();
        }
        return saveData.savedLayouts[index];
    }

    //Save data to save number
    public void SetSave(int index, int lives, int money, int level, string game, Bot bot)
    {
        SaveData newData = new SaveData();
        newData.lives = lives;
        newData.money = money;
        newData.level = level;
        newData.fuel = bot.GetSavedResource(ResourceType.Red, false);
        newData.blue = bot.GetSavedResource(ResourceType.Blue, false);
        newData.green = bot.GetSavedResource(ResourceType.Green, false);
        newData.yellow = bot.GetSavedResource(ResourceType.Yellow, false);
        newData.grey = bot.GetSavedResource(ResourceType.Grey, false);
        newData.hangarRed = bot.GetSavedResource(ResourceType.Red, true);
        newData.hangarBlue = bot.GetSavedResource(ResourceType.Blue, true);
        newData.hangarGreen = bot.GetSavedResource(ResourceType.Green, true);
        newData.hangarYellow = bot.GetSavedResource(ResourceType.Yellow, true);
        newData.hangarGrey = bot.GetSavedResource(ResourceType.Grey, true);
        newData.game = game;

        //~

        Sprite[,] botMap = bot.GetTileMap();
        newData.bot = new BotData[botMap.GetLength(0)];
        for(int x = 0;x < botMap.GetLength(0);x++)
        {
            newData.bot[x] = new BotData();
            newData.bot[x].botRow = new string[botMap.GetLength(1)];
            for(int y = 0;y< botMap.GetLength(1);y++)
            {
                newData.bot[x].botRow[y] = botMap[x, y] ? botMap[x, y].name : "";
            }
        }

        saveData.SaveData(newData, index);
        SaveGame();
    }

    //Save layout to save number
    public void SetLayout(int index, Sprite[,] bot)
    {
        SaveData newData = new SaveData();
        newData.lives = 0;
        newData.money = 0;
        newData.level = 0;
        newData.fuel = 0;
        newData.blue = 0;
        newData.green = 0;
        newData.yellow = 0;
        newData.grey = 0;
        newData.hangarRed = 0;
        newData.hangarBlue = 0;
        newData.hangarGreen = 0;
        newData.hangarYellow = 0;
        newData.hangarGrey = 0;
        newData.game = "LAYOUT";

        //~

        newData.bot = new BotData[bot.GetLength(0)];
        for (int x = 0; x < bot.GetLength(0); x++)
        {
            newData.bot[x] = new BotData();
            newData.bot[x].botRow = new string[bot.GetLength(1)];
            for (int y = 0; y < bot.GetLength(1); y++)
            {
                newData.bot[x].botRow[y] = bot[x, y] ? bot[x, y].name : "";
            }
        }

        saveData.SaveLayout(newData, index);
        SaveGame();
    }

    //Locate saved data if we haven't loaded it in yet
    public void Init()
    {
        if (!Directory.Exists(Application.persistentDataPath + saveDirectory))
        {
            Directory.CreateDirectory(Application.persistentDataPath + saveDirectory);
        }
        fullSavePath = Application.persistentDataPath + saveDirectory + saveFile;
        LoadData();

        while (saveData.saveFiles.Count < maxSaveFiles)
        {
            SaveData newData = new SaveData();
            newData.lives = 0;
            newData.money = 0;
            newData.level = 0;
            newData.fuel = 0;
            newData.blue = 0;
            newData.green = 0;
            newData.yellow = 0;
            newData.grey = 0;
            newData.hangarRed = 0;
            newData.hangarBlue = 0;
            newData.hangarGreen = 0;
            newData.hangarYellow = 0;
            newData.hangarGrey = 0;
            newData.game = "";
            newData.bot = new BotData[1] { new BotData() };
            newData.bot[0].botRow = new string[1] { "" };
            newData.containers = new List<ContainerData>();
            saveData.saveFiles.Add(newData);
        }
        while (saveData.savedLayouts.Count < maxSaveFiles)
        {
            SaveData newData = new SaveData();
            newData.lives = 0;
            newData.money = 0;
            newData.level = 0;
            newData.fuel = 0;
            newData.blue = 0;
            newData.green = 0;
            newData.yellow = 0;
            newData.grey = 0;
            newData.hangarRed = 0;
            newData.hangarBlue = 0;
            newData.hangarGreen = 0;
            newData.hangarYellow = 0;
            newData.hangarGrey = 0;
            newData.game = "";
            newData.bot = new BotData[1] { new BotData() };
            newData.bot[0].botRow = new string[1] { "" };
            newData.containers = new List<ContainerData>();
            saveData.savedLayouts.Add(newData);
        }
    }

    //Generate initial save data
    void CreateNewData()
    {
        saveData = new GameData();
        while(saveData.saveFiles.Count < maxSaveFiles)
        {
            SaveData newData = new SaveData();
            newData.lives = 0;
            newData.money = 0;
            newData.level = 0;
            newData.fuel = 0;
            newData.blue = 0;
            newData.green = 0;
            newData.yellow = 0;
            newData.grey = 0;
            newData.hangarRed = 0;
            newData.hangarBlue = 0;
            newData.hangarGreen = 0;
            newData.hangarYellow = 0;
            newData.hangarGrey = 0;
            newData.game = "";
            newData.bot = new BotData[1] { new BotData() };
            newData.bot[0].botRow = new string[1] { "" };
            newData.containers = new List<ContainerData>();
            saveData.saveFiles.Add(newData);
        }
        while (saveData.savedLayouts.Count < maxSaveFiles)
        {
            SaveData newData = new SaveData();
            newData.lives = 0;
            newData.money = 0;
            newData.level = 0;
            newData.fuel = 0;
            newData.blue = 0;
            newData.green = 0;
            newData.yellow = 0;
            newData.grey = 0;
            newData.hangarRed = 0;
            newData.hangarBlue = 0;
            newData.hangarGreen = 0;
            newData.hangarYellow = 0;
            newData.hangarGrey = 0;
            newData.game = "";
            newData.bot = new BotData[1] { new BotData() };
            newData.bot[0].botRow = new string[1] { "" };
            newData.containers = new List<ContainerData>();
            saveData.savedLayouts.Add(newData);
        }
        SaveGame();
    }

    //Load in existing save data
    public void LoadData()
    {
        if (File.Exists(fullSavePath))
        {
            try
            {
                string fullSaveData = File.ReadAllText(fullSavePath);
                saveData = JsonUtility.FromJson<GameData>(fullSaveData);
                hasSaveData = true;
            }
            catch
            {
                CreateNewData();
            }
        }
        else
        {
            CreateNewData();
        }
    }

    //Save current game data
    public void SaveGame()
    {
        File.WriteAllText(fullSavePath, JsonUtility.ToJson(saveData));
        hasSaveData = true;
    }
}

//Serializable format for storing list of save files
[Serializable]
public class GameData
{
    public List<SaveData> saveFiles = new List<SaveData>();
    public List<SaveData> savedLayouts = new List<SaveData>();

    //Save new data over file at index
    public void SaveData(SaveData newFile, int index)
    {
        saveFiles[index] = newFile;
    }

    //Save new data over layout at index
    public void SaveLayout(SaveData newLayout, int index)
    {
        savedLayouts[index] = newLayout;
    }
}

//Serializable format for storing data for individual saved games
[Serializable]
public class SaveData
{
    public int lives;       //Remaining lives
    public int money;       //Collected money
    public int level;       //Level reached in chosen game
    public string game;     //Which Game settings is this save associated with?
    public float fuel;      //Stored fuel
    public float blue;      //Stored blue resource
    public float green;     //Stored green resource
    public float yellow;    //Stored yellow resource
    public float grey;      //Stored grey resource
    public float hangarRed; //Excess red resource
    public float hangarBlue; //Excess blue resource
    public float hangarGreen; //Excess green resource
    public float hangarYellow; //Excess yellow resource
    public float hangarGrey; //Excess grey resource
    public BotData[] bot;   //Bot tilemap for reloading save
    public List<ContainerData> containers;  //Containers in bot, to be properly rotated after building
}

//Separate serialization for containers which have unique characteristics
[Serializable]
public class ContainerData
{
    public Vector2Int coords;
    public float openDirection;
}

//Separate serialization for bot map as 2D array serialization isn't automatically supported
[Serializable]
public class BotData
{
    public string[] botRow;    //Create a new bot data for each row to artificially support a second array level
}