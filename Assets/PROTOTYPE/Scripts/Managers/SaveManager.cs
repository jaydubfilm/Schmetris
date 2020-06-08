using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

//Controls saving and loading of app data
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

    //Return bot layout from save number
    public SaveData GetLayout(int index)
    {
        if (!hasSaveData)
        {
            Init();
        }
        return saveData.savedLayouts[index];
    }

    //Return containers from save number
    public List<ContainerData> GetLayoutContainers(int index)
    {
        if(!hasSaveData)
        {
            Init();
        }
        return saveData.savedLayouts[index].containers;
    }

    //Save data to save number
    public void SetSave(int index, int lives, int money, int level, string game, Bot bot)
    {
        SaveData newData = new SaveData();
        newData.lives = lives;
        newData.money = money;
        newData.level = level;
        newData.totalReddite = bot.totalReddite;
        newData.totalBlueSalt = bot.totalBlueSalt;
        newData.totalGreenAlgae = bot.totalGreenAlgae;
        newData.totalYellectrons = bot.totalYellectrons;
        newData.totalGreyscale = bot.totalGreyscale;
        newData.game = game;
        newData.containers = bot.savedContainerData;

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
    public void SetLayout(int index, Sprite[,] bot, List<ContainerData> containers)
    {
        SaveData newData = new SaveData();
        newData.lives = 0;
        newData.money = 0;
        newData.level = 0;
        newData.totalReddite = 0;
        newData.totalBlueSalt = 0;
        newData.totalGreenAlgae = 0;
        newData.totalYellectrons = 0;
        newData.totalGreyscale = 0;
        newData.game = "LAYOUT";

        newData.containers = containers;
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
            newData.totalReddite = 0;
            newData.totalBlueSalt = 0;
            newData.totalGreenAlgae = 0;
            newData.totalYellectrons = 0;
            newData.totalGreyscale = 0;
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
            newData.totalReddite = 0;
            newData.totalBlueSalt = 0;
            newData.totalGreenAlgae = 0;
            newData.totalYellectrons = 0;
            newData.totalGreyscale = 0;
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
            newData.totalReddite = 0;
            newData.totalBlueSalt = 0;
            newData.totalGreenAlgae = 0;
            newData.totalYellectrons = 0;
            newData.totalGreyscale = 0;
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
            newData.totalReddite = 0;
            newData.totalBlueSalt = 0;
            newData.totalGreenAlgae = 0;
            newData.totalYellectrons = 0;
            newData.totalGreyscale = 0;
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
    public float totalReddite;      //Stored fuel
    public float totalBlueSalt;      //Stored blue resource
    public float totalGreenAlgae;     //Stored green resource
    public float totalYellectrons;    //Stored yellow resource
    public float totalGreyscale;      //Stored grey resource
    public BotData[] bot;   //Bot tilemap for reloading save
    public List<ContainerData> containers;  //Containers in bot, to be properly rotated after building
}

//Separate serialization for containers which have unique characteristics
[Serializable]
public class ContainerData
{
    public Vector2Int coords;   //Where is this container located on the bot?
    public float openDirection; //What direction is the container facing?
}

//Separate serialization for bot map as 2D array serialization isn't automatically supported
[Serializable]
public class BotData
{
    public string[] botRow;    //Create a new bot data for each row to artificially support a second array level
}