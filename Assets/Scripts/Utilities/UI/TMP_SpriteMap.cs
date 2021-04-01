using System;
using System.Collections.Generic;

namespace StarSalvager.Utilities.UI
{
    public static class TMP_SpriteMap
    {
        //Gear Icon
        //====================================================================================================================//

        public  const string GEAR_ICON = "<sprite=\"Gear\" name=\"Gear_1\">";
        
        //Material Icons
        //====================================================================================================================//

        private const string MATERIAL_ICONS = "MaterIalIcons_SS_ver2";

        internal static readonly Dictionary<BIT_TYPE, string> MaterialIcons = new Dictionary<BIT_TYPE, string>
        {
            {BIT_TYPE.GREEN, $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_4\">"},
            {BIT_TYPE.GREY, $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_3\">"},
            {BIT_TYPE.RED, $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_2\">"},
            {BIT_TYPE.BLUE, $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_1\">"},
            {BIT_TYPE.YELLOW, $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_0\">"},
        };

        //Game Pieces
        //====================================================================================================================//

        private const string GAME_PIECES = "GamePieces_Atlas_ver4";

        internal static string GetBitSprite(BIT_TYPE type, int level)
        {
            int typeBase;
            switch (type)
            {
                case BIT_TYPE.BLUE:
                    typeBase = 0;
                    break;
                case BIT_TYPE.RED:
                    typeBase = 1;
                    break;
                case BIT_TYPE.YELLOW:
                    typeBase = 2;
                    break;
                case BIT_TYPE.GREEN:
                    typeBase = 3;
                    break;
                case BIT_TYPE.GREY:
                    typeBase = 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var levelOffset = level * 5;

            return $"<sprite=\"{GAME_PIECES}\" name=\"{GAME_PIECES}_{typeBase + levelOffset}\">";
        }

        //Enemy Sprites
        //====================================================================================================================//
        
        private const string ENEMIES = "enemy_ALLSPRITES_1_v1";

        internal static string GetEnemySprite(string spriteName)
        {
            return string.IsNullOrEmpty(spriteName) ? string.Empty : $"<sprite=\"{ENEMIES}\" name=\"{spriteName}\">";
        }

        //Inputs
        //====================================================================================================================//

        private const string KEYBOARD_LIGHT = "Keryboard_Light";
        private const string KEYBOARD_DARK = "Keyboard_Dark";

        private const string SWITCH = "Switch";

        //TODO Need to consider the orientation
        internal static string GetInputSprite(string control, bool isLight = true)
        {
            var spriteSheet = isLight ? KEYBOARD_LIGHT : KEYBOARD_DARK;
            string controlSprite = string.Empty;
#if UNITY_STANDALONE

            switch (control)
            {
                case "up":
                    controlSprite = "upArrow";
                    break;
                case "right":
                    controlSprite = "rightArrow";
                    break;
                case "down":
                    controlSprite = "downArrow";
                    break;
                case "left":
                    controlSprite = "leftArrow";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(control), control, null);
            }
#else
                        throw new NotImplementedException();


#endif


            return $"<sprite=\"{spriteSheet}\" name=\"{spriteSheet}_{controlSprite}\">";
        }

    }

}