using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StarSalvager.AI;
using StarSalvager.Audio;
using StarSalvager.Cameras.Data;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities
{
    public class Console : Singleton<Console>
    {
        public static bool Open => Instance._isOpen;
        
        private Rect _consoleRect;
        private Rect _inputRect;

        private string _input;
        private string _consoleDisplay = string.Empty;

        private bool _isOpen;

        private List<string> _cmds;

        private int _cmdIndex = -1;

        private const int FONT_SIZE = 16;

        private readonly string[] COMMANDS =
        {
            string.Concat("add ", "currency ", "[BIT_TYPE | all] ", "[uint]").ToUpper(),
            string.Concat("add ", "component ", "[COMPONENT_TYPE | all] ", "[uint]").ToUpper(),
            string.Concat("add ", "gears ", "[uint]").ToUpper(),
            string.Concat("add ", "liquid ", "[BIT_TYPE | all] ", "[float]").ToUpper(),
            string.Concat("add ", "storage ", "parts ", "[PART_TYPE] ", "[Amount:int]").ToUpper(),
            string.Concat("add ", "storage ", "components ", "[COMPONENT_TYPE] ", "[Amount:int]").ToUpper(),
            "\n",
            string.Concat("clear ", "console").ToUpper(),
            string.Concat("clear ", "remotedata").ToUpper(),
            "\n",
            string.Concat("damage ", "bot ", "(x,y) ", "[float]").ToUpper(),
            "\n",
            string.Concat("destroy ", "brick ", "(x,y)").ToUpper(),
            string.Concat("destroy ", "enemies").ToUpper(),
            string.Concat("destroy ", "bot").ToUpper(),
            "\n",
            string.Concat("hide ", "bot").ToUpper(),
            string.Concat("hide ", "ui").ToUpper(),
            "\n",
            string.Concat("print ", "liquid").ToUpper(),
            string.Concat("print ", "currency").ToUpper(),
            string.Concat("print ", "bits").ToUpper(),
            string.Concat("print ", "parts").ToUpper(),
            string.Concat("print ", "components").ToUpper(),
            "\n",
            //string.Concat("set ", "bitprofile ", "[index:uint]").ToUpper(),
            string.Concat("set ", "bot ", "magnet ", "[uint]").ToUpper(),
            string.Concat("set ", "bot ", "heat ", "[0.0 - 100.0]").ToUpper(),
            string.Concat("set ", "bot ", "health ", "[0.0 - 1.0]").ToUpper(),
            string.Concat("set ", "columns ", "[uint]").ToUpper(),
            string.Concat("set ", "component ", "[COMPONENT_TYPE | all] ", "[uint]").ToUpper(),
            string.Concat("set ", "currency ", "[BIT_TYPE | all] ", "[uint]").ToUpper(),
            string.Concat("set ", "godmode ", "[bool]").ToUpper(),
            string.Concat("set ", "liquid ", "[BIT_TYPE | all] ", "[float]").ToUpper(),
            string.Concat("set ", "orientation ", "[Horizontal | Vertical]").ToUpper(),
            //string.Concat("set ", "partprofile ", "[index:uint]").ToUpper(),
            string.Concat("set ", "paused ", "[bool]").ToUpper(),
            string.Concat("set ", "testing ", "[bool]").ToUpper(),
            string.Concat("set ", "timescale ", "[0.0 - 2.0]").ToUpper(),
            string.Concat("set ", "timeleft ", "[float]").ToUpper(),
            string.Concat("set ", "volume ", "[0.0 - 1.0]").ToUpper(),
            "\n",
            string.Concat("spawn ", "bit ", "[BIT_TYPE] ",  "(x,y) ", "[uint]").ToUpper(),
            string.Concat("spawn ", "part ", "[PART_TYPE] ",  "(x,y) ", "[uint]").ToUpper(),
            string.Concat("spawn ", "component ", "[COMPONENT_TYPE] ",  "(x,y)").ToUpper(),
            string.Concat("spawn ", "enemy ", "[enemy_name : use _ instead of space]").ToUpper(),
            "\n",
            string.Concat("unlock ", "sectorwave ", "[sector : int] ", "[wave : int]").ToUpper(),
            "\n",
            "T0",
            "T1",
            "P"
        };

        //============================================================================================================//

        private const int OFFSET = FONT_SIZE;
        private Vector2 scroll;
        
        #region Unity Functions

        private void Start()
        {
            _cmds = new List<string>();
            //_consoleDisplay = new List<string>();

            _consoleRect = new Rect(0, 0, Screen.width, Screen.height / 3f);
            _inputRect = new Rect(0, Screen.height / 3f, Screen.width, 35);

        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F2)
            {
                _isOpen = !_isOpen;
            }

            if (!_isOpen)
            {
                GUI.FocusControl(null);
                return;
            }

            GUI.skin.textField.fontSize =
                GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = FONT_SIZE;
            GUI.skin.box.alignment = TextAnchor.LowerLeft;
            //GUI.skin.label.padding = new RectOffset(10,10,10,10);
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

            int lines = 0;
            var split = _consoleDisplay.Split('\n');
            if (split.Length > 0)
                lines = split.Length + 10;
            
            GUI.Box(_consoleRect, string.Empty);

            scroll = GUI.BeginScrollView(_consoleRect, scroll, new Rect(0, 0, Screen.width - 20, lines  * OFFSET),
                false, true);
            
            GUI.Label(new Rect(0,0, Screen.width, lines * OFFSET ), _consoleDisplay);
            
            GUI.EndScrollView(true);
            
            

            switch (Event.current.keyCode)
            {
                case KeyCode.Return when Event.current.type == EventType.KeyDown:
                    TryParseCommand(_input);
                    _input = string.Empty;
                    _cmdIndex = -1;
                    break;
                case KeyCode.UpArrow when Event.current.type == EventType.KeyDown:
                    NavigatePreviousCommands(1);
                    break;
                case KeyCode.DownArrow when Event.current.type == EventType.KeyDown:
                    NavigatePreviousCommands(-1);
                    break;
                case KeyCode.Escape when Event.current.type == EventType.KeyDown:
                    _isOpen = false;
                    return;
            }

            GUI.SetNextControlName("MyTextField");
            _input = GUI.TextField(_inputRect, _input);

            GUI.FocusControl("MyTextField");


        }

        #endregion // Unity Functions

        //============================================================================================================//

        private void TryParseCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return;

            _cmds.Insert(0, cmd);

            //TODO Try and execute the command here
            _consoleDisplay += $"\n> {cmd}";

            var split = cmd.Split(' ');

            switch (split[0].ToLower())
            {
                case "add":
                    ParseAddCommand(split);
                    break;
                case "clear":
                    ParseClearCommand(split);
                    break;
                case "damage":
                    ParseDamageCommand(split);
                    break;
                case "destroy":
                    ParseDestroyCommand(split);
                    break;
                case "help":
                    _consoleDisplay += GetHelpString();
                    break;
                case "hide":
                    ParseHideCommand(split);
                    break;
                case "print":
                    ParsePrintCommand(split);
                    break;
                case "reset":
                    SceneLoader.ResetCurrentScene();
                    break;
                case "set":
                    ParseSetCommand(split);
                    break;
                case "spawn":
                    ParseSpawnCommand(split);
                    break;
                case "unlock":
                    ParseUnlockCmd(split);
                    break;
                case "t0":
                    Time.timeScale = 0;
                    break;
                case "t1":
                    Time.timeScale = 1;
                    break;
                case "p":
                    GameTimer.SetPaused(!GameTimer.IsPaused);
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[0]);
                    break;
            }

            _consoleDisplay += "\n";
            
            int lines = _consoleDisplay.Split('\n').Length;

            if (lines * OFFSET < _consoleRect.height)
                return;
            
            scroll.y = lines * OFFSET;
        }

        private void NavigatePreviousCommands(int dir)
        {
            _cmdIndex = Mathf.Clamp(_cmdIndex + dir, -1, _cmds.Count - 1);

            _input = _cmdIndex < 0 ? string.Empty : _cmds[_cmdIndex];
        }

        //============================================================================================================//

        private void ParseAddCommand(string[] split)
        {
            BIT_TYPE bitType;
            int intAmount;
            
            switch (split[1].ToLower())
            {
                case "component":
                    if (!int.TryParse(split[3], out var compAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    if (split[2].ToLower().Equals("all"))
                    {
                        var componentData = new Dictionary<COMPONENT_TYPE, int>((IDictionary<COMPONENT_TYPE, int>) PlayerPersistentData.PlayerData.components);
                        
                        foreach (COMPONENT_TYPE value in Enum.GetValues(typeof(COMPONENT_TYPE)))
                        {
                            if (!componentData.ContainsKey(value))
                                continue;
                                
                            componentData[value] += compAmount;
                        }
                        
                        PlayerPersistentData.PlayerData.SetComponents(componentData);
                        
                    }
                    else if (Enum.TryParse(split[2], true, out COMPONENT_TYPE compType))
                    {
                        PlayerPersistentData.PlayerData.AddComponent(compType, compAmount);
                    }
                    else
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    PlayerData.OnValuesChanged?.Invoke();
                    break;
                case "currency":
                    /*if (!Enum.TryParse(split[2], true, out bit))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }

                    if (int.TryParse(split[3], out var intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }

                    PlayerPersistentData.PlayerData.resources[bit] += intAmount;*/
                    
                    if (!int.TryParse(split[3], out intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    if (split[2].ToLower().Equals("all"))
                    {
                        foreach (BIT_TYPE value in Enum.GetValues(typeof(BIT_TYPE)))
                        {
                            if (!PlayerPersistentData.PlayerData.resources.ContainsKey(value))
                                continue;

                            PlayerPersistentData.PlayerData.AddResource(value, intAmount);
                        }
                        
                    }
                    else if (Enum.TryParse(split[2], true, out bitType))
                    {

                        PlayerPersistentData.PlayerData.AddResource(bitType, intAmount);
                    }
                    else
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    
                    PlayerData.OnValuesChanged?.Invoke();

                    break;
                case "gears":
                    if (!int.TryParse(split[2], out intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    
                    PlayerPersistentData.PlayerData.ChangeGears(intAmount);
                    
                    break;
                case "liquid":
                    if (!float.TryParse(split[3], out var floatAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    if (split[2].ToLower().Equals("all"))
                    {
                        foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
                        {
                            if (!PlayerPersistentData.PlayerData.resources.ContainsKey(_bitType))
                                continue;

                            //I dont want to use AddLiquidResource() here because it would call the OnValuesChanged callback too much

                            PlayerPersistentData.PlayerData.AddLiquidResource(_bitType, floatAmount, false);
                        }
                        
                    }
                    else if (Enum.TryParse(split[2], true, out bitType))
                    {
                        PlayerPersistentData.PlayerData.AddLiquidResource(bitType, floatAmount, false);
                    }
                    else
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }

                    PlayerData.OnValuesChanged?.Invoke();
                    break;
                case "storage":
                    if (!int.TryParse(split[4], out var addAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[4]);
                        break;
                    }
                    
                    switch (split[2].ToLower())
                    {
                        case "components":
                            if (Enum.TryParse(split[3], true, out COMPONENT_TYPE compType))
                            {
                                PlayerPersistentData.PlayerData.AddComponent(compType, addAmount);
                                break;
                            }

                            _consoleDisplay += UnrecognizeCommand(split[3]);
                            break;
                        case "parts":
                            if (Enum.TryParse(split[3], true, out PART_TYPE partType))
                            {
                                var partBlockData = new BlockData
                                {
                                    ClassType = nameof(Part),
                                    Type = (int) partType,
                                    Level = 0,
                                    Health = FactoryManager.Instance
                                        .PartsRemoteData
                                        .GetRemoteData(partType)
                                        .levels[0]
                                        .health
                                };
                                
                                for (var i = 0; i < addAmount; i++)
                                {
                                    PlayerPersistentData.PlayerData.AddPartToStorage(partBlockData);
                                }
                                break;
                            }

                            _consoleDisplay += UnrecognizeCommand(split[3]);
                            break;
                        default:
                            _consoleDisplay += UnrecognizeCommand(split[2]);
                            break;
                    }
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }
        
        private void ParseClearCommand(string[] split)
        {
            switch (split[1].ToLower())
            {
                case "console":
                    _consoleDisplay = string.Empty;
                    _cmds.Clear();
                    break;
                case "remote" when split.Length > 2 && split[2].ToLower().Equals("data"):
                case "remotedata":
                    Files.ClearRemoteData();
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }
        
        private void ParseDamageCommand(string[] split)
        {
            switch (split[1].ToLower())
            {
                case "bot":
                    if (!Vector2IntExtensions.TryParseVector2Int(split[2], out var coord))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }
                    if (!float.TryParse(split[3], out var damage))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    var bot = FindObjectOfType<Bot>();

                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        return;
                    }

                    var brick = bot.attachedBlocks.FirstOrDefault(x => x.Coordinate == coord);

                    if (brick == null)
                    {
                        _consoleDisplay += $"\nERROR. No attachable found at {coord}.";
                        return;
                    }
                    
                    bot.TryHitAt(brick, damage);
                    
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }
        
        private void ParseDestroyCommand(string[] split)
        {
            Bot bot;
            switch (split[1].ToLower())
            {
                case "brick":
                    if (!Vector2IntExtensions.TryParseVector2Int(split[2], out var coord))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }

                    bot = FindObjectOfType<Bot>();
                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        break;
                    }

                    var brick = bot.attachedBlocks.FirstOrDefault(x => x.Coordinate == coord);
                    
                    if (brick == null)
                    {
                        _consoleDisplay += $"\nError. No Brick found at {coord}";
                        break;
                    }
                    
                    bot.TryHitAt(brick, 100000f);
                    
                    break;
                case "enemies":

                    var enemies = FindObjectsOfType<Enemy>().Where(e => e.IsRecycled == false).OfType<IHealth>();

                    foreach (var enemy in enemies)
                    {
                        enemy.ChangeHealth(-100000f);
                    }
                    
                    break;
                case "bot":
                    bot = FindObjectOfType<Bot>();

                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        break;
                    }
                    
                    bot.TryHitAt(bot.attachedBlocks[0], 100000f);
                    
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }
        
        private void ParseHideCommand(string[] split)
        {
            bool state;
            
            switch (split[1].ToLower())
            {
                case "ui":
                    if (!TryParseBool(split[2], out state))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }
                    
                    var canvases = FindObjectsOfType<Canvas>();

                    foreach (var canvas in canvases)
                    {
                        canvas.enabled = !state;
                    }
                    
                    break;
                case "bot":
                    if (!TryParseBool(split[2], out state))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }

                    var bot = FindObjectOfType<Bot>();
                    bot.enabled = state;
                    
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }

        private void ParsePrintCommand(string[] split)
        {
            switch (split[1].ToLower())
            {
                case "liquid":
                    _consoleDisplay += $"\n{GetDictionaryAsString(PlayerPersistentData.PlayerData.liquidResource)}";
                    break;
                case "currency":
                    _consoleDisplay += $"\n{GetDictionaryAsString(PlayerPersistentData.PlayerData.resources)}";
                    break;
                case "bits":
                    _consoleDisplay += $"\n{GetEnumsAsString<BIT_TYPE>()}";
                    break;
                case "parts":
                    _consoleDisplay += $"\n{GetEnumsAsString<PART_TYPE>()}";
                    break;
                case "components":
                    _consoleDisplay += $"\n{GetEnumsAsString<COMPONENT_TYPE>()}";
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }

        private void ParseSetCommand(string[] split)
        {
            int intAmount;
            bool state;
            BIT_TYPE bitType;
            BotPartsLogic botPartsLogic;
            Bot bot;

            switch (split[1].ToLower())
            {
                /*case "bitprofile":
                    if (!int.TryParse(split[2], out intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    
                    //FactoryManager.Instance?.ChangeBitProfile(intAmount);
                    
                    break;*/
                case "bot":
                {
                    switch (split[2].ToLower())
                    {
                        case "magnet":
                            if (!int.TryParse(split[3], out var magnet))
                            {
                                _consoleDisplay += UnrecognizeCommand(split[3]);
                                break;
                            }

                            botPartsLogic = FindObjectOfType<BotPartsLogic>();

                            if (botPartsLogic == null)
                            {
                                _consoleDisplay += NoActiveObject(typeof(Bot));
                                return;
                            }

                            botPartsLogic.SetMagnetOverride(magnet);
                    
                            //PlayerPersistentData.PlayerData.liquidResource[bit] = amount;
                    
                            break;
                        case "heat":
                            if (!float.TryParse(split[3], out var heat))
                            {
                                _consoleDisplay += UnrecognizeCommand(split[3]);
                                break;
                            }

                            botPartsLogic = FindObjectOfType<BotPartsLogic>();

                            if (botPartsLogic == null)
                            {
                                _consoleDisplay += NoActiveObject(typeof(Bot));
                                return;
                            }

                            botPartsLogic.coreHeat = heat;
                            break;
                        case "health":
                            if (!float.TryParse(split[3], out var health))
                            {
                                _consoleDisplay += UnrecognizeCommand(split[3]);
                                break;
                            }

                            bot = FindObjectOfType<Bot>();

                            if (bot == null)
                            {
                                _consoleDisplay += NoActiveObject(typeof(Bot));
                                return;
                            }

                            var attachables = bot.attachedBlocks.OfType<IHealth>();

                            foreach (var attachable in attachables)
                            {
                                attachable.SetupHealthValues(attachable.StartingHealth, attachable.StartingHealth * health);
                            }
                            
                            
                            break;
                        
                    }

                    break;
                }
                case "columns":
                    if (!int.TryParse(split[2], out var columns))
                    {
                        UnrecognizeCommand(split[2]);
                        break;
                    }

                    Globals.ScaleCamera(columns);
                    break;
                case "currency":


                    if (!int.TryParse(split[3], out intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    if (split[2].ToLower().Equals("all"))
                    {
                        foreach (BIT_TYPE value in Enum.GetValues(typeof(BIT_TYPE)))
                        {
                            if (!PlayerPersistentData.PlayerData.resources.ContainsKey(value))
                                continue;

                            PlayerPersistentData.PlayerData.SetResources(value, intAmount);
                        }
                    }
                    else if (Enum.TryParse(split[2], true, out bitType))
                    {
                        PlayerPersistentData.PlayerData.SetResources(bitType, intAmount);
                    }
                    else
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }

                    
                    PlayerData.OnValuesChanged?.Invoke();

                    break;
                case "component":

                    if (!int.TryParse(split[3], out var compAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    if (split[2].ToLower().Equals("all"))
                    {
                        var componentData = new Dictionary<COMPONENT_TYPE, int>((IDictionary<COMPONENT_TYPE, int>) PlayerPersistentData.PlayerData.components);
                        
                        foreach (COMPONENT_TYPE value in Enum.GetValues(typeof(COMPONENT_TYPE)))
                        {
                            if (!componentData.ContainsKey(value))
                                continue;
                                
                            componentData[value] = compAmount;
                        }
                        
                        PlayerPersistentData.PlayerData.SetComponents(componentData);
                    }
                    else if (Enum.TryParse(split[2], true, out COMPONENT_TYPE compType))
                    {
                        PlayerPersistentData.PlayerData.SetComponents(compType, compAmount);
                    }
                    else
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }

                    
                    PlayerData.OnValuesChanged?.Invoke();

                    break;
                case "godmode":

                    if (!TryParseBool(split[2], out state))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }

                    bot = FindObjectOfType<Bot>();

                    if (bot)
                        bot.PROTO_GodMode = state;
                    else
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                    }

                    break;
                case "liquid":
                    if (!float.TryParse(split[3], out var floatAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    if (split[2].ToLower().Equals("all"))
                    {
                        var data = new Dictionary<BIT_TYPE, float>((IDictionary<BIT_TYPE, float>) PlayerPersistentData.PlayerData.liquidResource);

                        foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
                        {
                            if (!data.ContainsKey(_bitType))
                                continue;
                            
                            data[_bitType] = floatAmount;
                        }

                        PlayerPersistentData.PlayerData.SetLiquidResource(data, true);
                        PlayerPersistentData.PlayerData.SetLiquidResource(data, false);
                        
                    }
                    else if (Enum.TryParse(split[2], true, out bitType))
                    {
                        if (!PlayerPersistentData.PlayerData.liquidResource.ContainsKey(bitType))
                            break;
                        
                        PlayerPersistentData.PlayerData.SetLiquidResource(bitType, floatAmount, true);
                        PlayerPersistentData.PlayerData.SetLiquidResource(bitType, floatAmount, false);
                    }
                    else
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }
                    break;
                case "orientation":
                    switch (split[2].ToLower())
                    {
                        case "v":
                        case "vertical":
                            Globals.Orientation = ORIENTATION.VERTICAL;
                            break;
                        case "h":
                        case "horizontal":
                            Globals.Orientation = ORIENTATION.HORIZONTAL;
                            break;
                        default:
                            _consoleDisplay +=UnrecognizeCommand(split[2]);
                            break;
                    }

                    break;
                /*case "partprofile":
                    if (!int.TryParse(split[2], out intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    
                    //FactoryManager.Instance?.ChangePartProfile(intAmount);
                    
                    break;*/
                case "paused":
                    if (!TryParseBool(split[2], out state))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }

                    GameTimer.SetPaused(state);
                    break;
                case "timeleft":
                    if (!float.TryParse(split[2], out var timeLeft))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }
                    
                    LevelManager.Instance.ForceSetTimeRemaining(timeLeft);
                    break;
                case "testing":
                    if (!TryParseBool(split[2], out state))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }

                    Globals.DisableTestingFeatures = state;
                    break;
                case "timescale":
                    if (!float.TryParse(split[2], out var scale))
                    {
                        _consoleDisplay +=UnrecognizeCommand(split[2]);
                        break;
                    }

                    Time.timeScale = scale;
                    break;
                case "volume":
                    if (!float.TryParse(split[2], out var volume))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    //_consoleDisplay += "\nVolume is not yet implemented";
                    AudioController.SetVolume(volume);
                    
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }
        
        private void ParseSpawnCommand(string[] split)
        {
            Bot bot;
            Vector2Int coord;
            int lvl;
            switch (split[1].ToLower())
            {
                case "bit":
                    if (!Enum.TryParse(split[2], true, out BIT_TYPE bit))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    if (!Vector2IntExtensions.TryParseVector2Int(split[3], out coord))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    if (split.Length >= 5)
                    {
                        if (!int.TryParse(split[4], out lvl))
                        {
                            _consoleDisplay += UnrecognizeCommand(split[4]);
                            break;
                        }
                    }
                    else
                    {
                        lvl = 0;
                    }

                    bot = FindObjectOfType<Bot>();
                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        return;
                    }

                    var newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                        .CreateObject<IAttachable>(bit, lvl);
                    
                    bot.AttachNewBlock(coord, newBit, true, true, false);
                    break;
                case "part":
                    if (!Enum.TryParse(split[2], true, out PART_TYPE part))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    if (!Vector2IntExtensions.TryParseVector2Int(split[3], out coord))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }

                    if (split.Length >= 5)
                    {
                        if (!int.TryParse(split[4], out lvl))
                        {
                            _consoleDisplay += UnrecognizeCommand(split[4]);
                            break;
                        }
                    }
                    else
                    {
                        lvl = 0;
                    }
                    

                    bot = FindObjectOfType<Bot>();
                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        return;
                    }

                    var newPart = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                        .CreateObject<IAttachable>(part, lvl);
                    
                    bot.AttachNewBlock(coord, newPart, true, true, false);
                    break;
                case "component":
                    if (!Enum.TryParse(split[2], true, out COMPONENT_TYPE component))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                        break;
                    }
                    if (!Vector2IntExtensions.TryParseVector2Int(split[3], out coord))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }

                    bot = FindObjectOfType<Bot>();
                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        return;
                    }

                    var newComponent = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>()
                        .CreateObject<IAttachable>(component);
                    
                    bot.AttachNewBlock(coord, newComponent, true, true, false);
                    break;
                case "enemy":
                    var type = split[2].Replace('_', ' ');

                    if (!int.TryParse(split[3], out var count))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    
                    var manager = FindObjectOfType<EnemyManager>();
                    if (manager == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(EnemyManager));
                        return;
                    }

                    for (var i = 0; i < count; i++)
                    {
                        var enemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObjectName<Enemy>(type);
                        manager.AddEnemy(enemy);
                    }
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }

        private void ParseUnlockCmd(string[] split)
        {
            switch (split[1].ToLower())
            {
                case "sectorwave":
                    if (!int.TryParse(split[2], out var sector))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }
                    if (!int.TryParse(split[3], out var wave))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                        break;
                    }

                    PlayerPersistentData.PlayerData.PlayerPreviouslyCompletedNodes.Add(PlayerPersistentData.PlayerData.LevelRingNodeTree.ConvertSectorWaveToNodeIndex(sector, wave));
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
            
        }

        private string GetHelpString()
        {
            return COMMANDS.Aggregate("\n\nThese are commands:\n=================================\n",
                (current, s) => current + $"{s}\n");
        }
        

        //============================================================================================================//

        private static string GetDictionaryAsString<U, T>(Dictionary<U, T> dictionary)
        {
            return dictionary.Aggregate("", (current, o) => current + $"[{o.Key}] => {o.Value}\n");
        }
        private static string GetDictionaryAsString<U, T>(IReadOnlyDictionary<U, T> dictionary)
        {
            return dictionary.Aggregate("", (current, o) => current + $"[{o.Key}] => {o.Value}\n");
        }

        private static string GetEnumsAsString<E>() where E : Enum
        {
            string[] names = Enum.GetNames(typeof(E));

            return string.Join(", ", names);
        }

        private static string UnrecognizeCommand(string cmd)
        {
            return $"\nUnrecognized command at '{cmd}'. Enter 'help' to see possible commands";
        }
        
        private static string NoActiveObject(Type type)
        {
            return $"\nError. No active {nameof(type)} found.";
        }

        private static bool TryParseBool(string s, out bool value)
        {
            value = false;
            
            switch (s.ToLower())
            {
                case "1":
                case "t":
                case "true":
                    value = true;
                    return true;
                case "0":
                case "f":
                case "false":
                    value = false;
                    return true;
                default:
                    return false;
            }
        }

    }
}

