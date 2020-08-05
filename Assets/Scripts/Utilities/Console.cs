using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.AI;
using StarSalvager.Cameras.Data;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using UnityEngine;

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
            string.Concat("add ", "currency ", "[BIT_TYPE] ", "[uint]").ToUpper(),
            string.Concat("add ", "liquid ", "[BIT_TYPE] ", "[float]").ToUpper(),
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
            "\n",
            string.Concat("set ", "bot ", "magnet ", "[uint]").ToUpper(),
            string.Concat("set ", "bot ", "heat ", "[0.0 - 100.0]").ToUpper(),
            string.Concat("set ", "bot ", "health ", "[0.0 - 1.0]").ToUpper(),
            string.Concat("set ", "columns ", "[uint]").ToUpper(),
            string.Concat("set ", "currency ", "[BIT_TYPE] ", "[uint]").ToUpper(),
            string.Concat("set ", "godmode ", "[bool]").ToUpper(),
            string.Concat("set ", "liquid ", "[BIT_TYPE] ", "[float]").ToUpper(),
            string.Concat("set ", "orientation ", "[Horizontal | Vertical]").ToUpper(),
            string.Concat("set ", "paused ", "[bool]").ToUpper(),
            string.Concat("set ", "timescale ", "[0.0 - 2.0]").ToUpper(),
            string.Concat("set ", "volume ", "[0.0 - 1.0]").ToUpper(),
            "\n",
            string.Concat("spawn ", "bit ", "[BIT_TYPE] ",  "(x,y)").ToUpper(),
            string.Concat("spawn ", "part ", "[PART_TYPE] ",  "(x,y)").ToUpper(),
            string.Concat("spawn ", "component ", "[COMPONENT_TYPE] ",  "(x,y)").ToUpper(),
            string.Concat("spawn ", "enemy ", "[enemy_name : use _ instead of space]").ToUpper(),
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
            BIT_TYPE bit;
            
            switch (split[1].ToLower())
            {
                case "currency":
                    if (!Enum.TryParse(split[2], true, out bit))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }

                    if (int.TryParse(split[3], out var intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }

                    PlayerPersistentData.PlayerData.resources[bit] += intAmount;
                    
                    break;
                case "liquid":
                    if (!Enum.TryParse(split[2], true, out bit))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }

                    if (float.TryParse(split[3], out var amount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }

                    PlayerPersistentData.PlayerData.liquidResource[bit] += amount;
                    
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
                case "remotedata":
                    FactoryManager.ClearRemoteData();
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
                        UnrecognizeCommand(split[2]);
                    }
                    if (!float.TryParse(split[3], out var damage))
                    {
                        UnrecognizeCommand(split[3]);
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
                        UnrecognizeCommand(split[2]);
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
                        UnrecognizeCommand(split[2]);
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
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }

        private void ParseSetCommand(string[] split)
        {
            bool state;
            BIT_TYPE bit;
            BotPartsLogic botPartsLogic;
            Bot bot;

            switch (split[1].ToLower())
            {
                case "bot":
                {
                    switch (split[2].ToLower())
                    {
                        case "magnet":
                            if (int.TryParse(split[3], out var magnet))
                            {
                                _consoleDisplay += UnrecognizeCommand(split[3]);
                            }

                            botPartsLogic = FindObjectOfType<BotPartsLogic>();

                            if (botPartsLogic == null)
                            {
                                _consoleDisplay += NoActiveObject(typeof(Bot));
                                return;
                            }

                            botPartsLogic.SetMagentOverride(magnet);
                    
                            //PlayerPersistentData.PlayerData.liquidResource[bit] = amount;
                    
                            break;
                        case "heat":
                            if (float.TryParse(split[3], out var heat))
                            {
                                _consoleDisplay += UnrecognizeCommand(split[3]);
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
                            if (float.TryParse(split[3], out var health))
                            {
                                _consoleDisplay += UnrecognizeCommand(split[3]);
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
                    if (!Enum.TryParse(split[2], true, out bit))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }

                    int intAmount;
                    if (int.TryParse(split[3], out intAmount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }

                    PlayerPersistentData.PlayerData.resources[bit] = intAmount;
                    
                    break;
                case "godmode":

                    if (!TryParseBool(split[2], out state))
                    {
                        UnrecognizeCommand(split[2]);
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
                    if (!Enum.TryParse(split[2], true, out bit))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }

                    float amount;
                    if (float.TryParse(split[3], out amount))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }

                    PlayerPersistentData.PlayerData.liquidResource[bit] = amount;
                    
                    break;
                case "orientation":
                    switch (split[2])
                    {
                        case "V":
                        case "Vertical":
                            Globals.Orientation = ORIENTATION.VERTICAL;
                            break;
                        case "H":
                        case "Horizontal":
                            Globals.Orientation = ORIENTATION.HORIZONTAL;
                            break;
                    }

                    break;
                case "paused":
                    if (!TryParseBool(split[2], out state))
                    {
                        UnrecognizeCommand(split[2]);
                        break;
                    }

                    GameTimer.SetPaused(state);
                    break;
                case "timescale":
                    if (!float.TryParse(split[2], out var scale))
                    {
                        UnrecognizeCommand(split[2]);
                        break;
                    }

                    Time.timeScale = scale;
                    break;
                case "volume":
                    _consoleDisplay += "\nVolume is not yet implemented";
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
                    }
                    if (!Vector2IntExtensions.TryParseVector2Int(split[3], out coord))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }
                    if (!int.TryParse(split[4], out lvl))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[4]);
                    }

                    bot = FindObjectOfType<Bot>();
                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        return;
                    }

                    var newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                        .CreateObject<IAttachable>(bit, lvl);
                    
                    bot.AttachNewBit(coord, newBit, true, true, false);
                    break;
                case "part":
                    if (!Enum.TryParse(split[2], true, out PART_TYPE part))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }
                    if (!Vector2IntExtensions.TryParseVector2Int(split[3], out coord))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }
                    if (!int.TryParse(split[4], out lvl))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[4]);
                    }

                    bot = FindObjectOfType<Bot>();
                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        return;
                    }

                    var newPart = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                        .CreateObject<IAttachable>(part, lvl);
                    
                    bot.AttachNewBit(coord, newPart, true, true, false);
                    break;
                case "component":
                    if (!Enum.TryParse(split[2], true, out COMPONENT_TYPE component))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[2]);
                    }
                    if (!Vector2IntExtensions.TryParseVector2Int(split[3], out coord))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
                    }

                    bot = FindObjectOfType<Bot>();
                    if (bot == null)
                    {
                        _consoleDisplay += NoActiveObject(typeof(Bot));
                        return;
                    }

                    var newComponent = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>()
                        .CreateObject<IAttachable>(component);
                    
                    bot.AttachNewBit(coord, newComponent, true, true, false);
                    break;
                case "enemy":
                    var type = split[2].Replace('_', ' ');

                    if (!int.TryParse(split[3], out var count))
                    {
                        _consoleDisplay += UnrecognizeCommand(split[3]);
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

