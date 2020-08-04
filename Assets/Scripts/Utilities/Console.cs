using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Cameras.Data;
using StarSalvager.Factories;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public class Console : Singleton<Console>
    {
        private Rect _consoleRect;
        private Rect _inputRect;

        private string _input;
        private string _consoleDisplay;

        private bool _isOpen;

        private List<string> _cmds;

        private int _cmdIndex = -1;

        private const int FONT_SIZE = 16;
        
        private const string HELP_DATA = "\nThese are commands:\n=================================" +
                                         "\n set orientation <b>[H|V|Horizontal|Vertical]</b>" +
                                         "\n set columns <b>[int]</b>" +
                                         "\n set volume <b>[0.0 - 1.0]</b>" +
                                         "\n set godmode <b>[True|False|T|F|1|0]</b>" +
                                         "\n set paused <b>[True|False|T|F|1|0]</b>" +
                                         "\n set timescale <b>[0.0 - 2.0]</b>" +
                                         "\n\n set currency <b>[BIT_TYPE] [uint]</b>" +
                                         "\n add currency <b>[BIT_TYPE] [uint]</b>" +
                                         "\n\n set liquid <b>[BIT_TYPE] [uint]</b>" +
                                         "\n add liquid <b>[BIT_TYPE] [uint]</b>" +
                                         "\n\n clear <b>[console|remotedata]</b>";

        //============================================================================================================//

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
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;


            GUI.Box(_consoleRect, _consoleDisplay);

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



        //Intended Commands
        //==========================//
        // set orientation [H|V|Horizontal|Vertical]
        // set columns [int]
        // set volume [0.0 - 1.0]
        // set godmode [True|False|T|F]

        // set paused [True|False|T|F]
        // set timescale [0.0 - 2.0]

        // set currency [BIT_TYPE] [uint]
        // add currency [BIT_TYPE] [uint]
        
        // set liquid [BIT_TYPE] [uint]
        // add liquid [BIT_TYPE] [uint]

        // clear [console|remotedata]

        // help

        private void TryParseCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return;

            _cmds.Insert(0, cmd);

            //TODO Try and execute the command here
            _consoleDisplay += $"\n{cmd}";

            var split = cmd.Split(' ');

            switch (split[0])
            {
                case "set":
                    ParseSetCommand(split);
                    break;
                case "add":
                    ParseAddCommand(split);
                    break;
                case "clear":
                    ParseClearCommand(split);
                    break;
                case "print":
                    ParsePrintCommand(split);
                    break;
                case "help":
                    _consoleDisplay += HELP_DATA;
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[0]);
                    break;
            }
        }

        private void NavigatePreviousCommands(int dir)
        {
            _cmdIndex = Mathf.Clamp(_cmdIndex + dir, -1, _cmds.Count - 1);

            _input = _cmdIndex < 0 ? string.Empty : _cmds[_cmdIndex];
        }

        //============================================================================================================//

        private void ParseSetCommand(string[] split)
        {
            bool state;
            BIT_TYPE bit;

            switch (split[1])
            {
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
                case "columns":
                    if (!int.TryParse(split[2], out var columns))
                    {
                        UnrecognizeCommand(split[2]);
                        break;
                    }

                    Globals.ScaleCamera(columns);
                    break;
                case "volume":
                    _consoleDisplay += "\nVolume is not yet implemented";
                    break;
                case "godmode":

                    if (!TryParseBool(split[2], out state))
                    {
                        UnrecognizeCommand(split[2]);
                        break;
                    }

                    var bot = FindObjectOfType<Bot>();

                    if (bot)
                        bot.PROTO_GodMode = state;
                    else
                    {
                        _consoleDisplay += "\nNo Bot found to enable God Mode";
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
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }

        private void ParseAddCommand(string[] split)
        {
            BIT_TYPE bit;
            
            switch (split[1])
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
            switch (split[1])
            {
                case "console":
                    _consoleDisplay = string.Empty;
                    break;
                case "remotedata":
                    FactoryManager.ClearRemoteData();
                    break;
                default:
                    _consoleDisplay += UnrecognizeCommand(split[1]);
                    break;
            }
        }

        private void ParsePrintCommand(string[] split)
        {
            switch (split[1])
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


        //============================================================================================================//

        private static string GetDictionaryAsString<U, T>(Dictionary<U, T> dictionary)
        {
            return dictionary.Aggregate("", (current, o) => current + $"[{o.Key}] => {o.Value}\n");
        }

        private static string UnrecognizeCommand(string cmd)
        {
            return $"\nUnrecognized command at '{cmd}'. Enter 'help' to see possible commands";
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

