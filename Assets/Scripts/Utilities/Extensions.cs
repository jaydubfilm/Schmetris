using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public static class Extensions
    {
        //============================================================================================================//
        
        private static readonly Vector2Int[] DirectionVectors = {
            new Vector2Int(-1, 0),   //LEFT
            new Vector2Int(0, 1),    //UP
            new Vector2Int(1, 0),    //RIGHT
            new Vector2Int(0, -1)    //DOWN
        };
        
        public static Vector2Int ToVector2Int(this DIRECTION direction)
        {
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.UP:
                case DIRECTION.RIGHT:
                case DIRECTION.DOWN:
                    return DirectionVectors[(int) direction];
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        public static DIRECTION ToDirection(this Vector2Int vector2Int)
        {
            if(vector2Int == Vector2Int.zero)
                throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");

            if (vector2Int.x == 0)
            {
                return vector2Int.y > 0 ? DIRECTION.UP : DIRECTION.DOWN;
            }
            
            if (vector2Int.y == 0)
            {
                return vector2Int.x > 0 ? DIRECTION.RIGHT : DIRECTION.LEFT;
            }
            
            throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");
        }
        
        //============================================================================================================//
        
        public static string ExportLayout(this Bot bot)
        {
            //TODO Need to consider that there will be parts & bits attached to the bot

            var data = bot.attachedBlocks
                .Select(b => b.ToBlockData())
                .ToArray();

            var blah = JsonConvert.SerializeObject(data, Formatting.None);
            
            Debug.Log(blah);
            
            
            return string.Empty;
        }
        public static void ImportLayout(this Bot bot, string jsonLayout)
        {
            var loadedBlocks = JsonConvert.DeserializeObject<List<BlockData>>(jsonLayout);

            foreach (var block in loadedBlocks)
            {
                AttachableBase attachable;
                switch (block.ClassType)
                {
                    case nameof(Bit):
                        attachable = AttachableFactory.Instance.GetFactory<BitFactory>().CreateObject<AttachableBase>(block);
                        break;
                    case nameof(Part):
                        attachable = AttachableFactory.Instance.GetFactory<PartFactory>().CreateObject<AttachableBase>(block);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(block.ClassType), block.ClassType, null);
                }
                
                bot.AttachNewBit(attachable.Coordinate, attachable);
            }
            
        }
        
    }
}

