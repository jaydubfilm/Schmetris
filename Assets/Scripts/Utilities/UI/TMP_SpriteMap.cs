using System;
using System.Collections.Generic;

namespace StarSalvager.Utilities.UI
{
    public static class TMP_SpriteMap
    {
        //Material Icons
        //====================================================================================================================//
        
        private const string MATERIAL_ICONS = "MaterIalIcons_SS_ver2";
        internal static readonly Dictionary<BIT_TYPE, string> MaterialIcons = new Dictionary<BIT_TYPE, string>
        {
            { BIT_TYPE.GREEN,  $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_4\">" },
            { BIT_TYPE.GREY,   $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_3\">" },
            { BIT_TYPE.RED,    $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_2\">" },
            { BIT_TYPE.BLUE,   $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_1\">" },
            { BIT_TYPE.YELLOW, $"<sprite=\"{MATERIAL_ICONS}\" name=\"{MATERIAL_ICONS}_0\">" },
        };

        //Game Pieces
        //====================================================================================================================//
        
        private const string GAME_PIECES = "GamePieces_Atlas";

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

        //====================================================================================================================//
        
    }

}