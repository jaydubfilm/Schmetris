using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class BotExtensions
    {
        //============================================================================================================//
        
        #region Import/Export
        
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
                        attachable = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<AttachableBase>(block);
                        break;
                    case nameof(Part):
                        attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<AttachableBase>(block);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(block.ClassType), block.ClassType, null);
                }
                
                bot.AttachNewBit(attachable.Coordinate, attachable);
            }
            
        }
        
        #endregion //Import/Export
        
        //============================================================================================================//
        
        #region Obtaining Bits
        
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAttachablesAround<T>(this Bot bot, AttachableBase from) where T: AttachableBase
        {
            return new List<T>
            {
                bot.GetAttachableInDirectionOf<T>(from, DIRECTION.LEFT),
                bot.GetAttachableInDirectionOf<T>(from, DIRECTION.UP),
                bot.GetAttachableInDirectionOf<T>(from, DIRECTION.RIGHT),
                bot.GetAttachableInDirectionOf<T>(from, DIRECTION.DOWN)
            };
        }
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Vector2Int> GetCoordinatesAround(this Bot bot, AttachableBase from)
        {
            var check = new List<AttachableBase>
            {
                bot.GetAttachableInDirectionOf<AttachableBase>(from, DIRECTION.LEFT),
                bot.GetAttachableInDirectionOf<AttachableBase>(from, DIRECTION.UP),
                bot.GetAttachableInDirectionOf<AttachableBase>(from, DIRECTION.RIGHT),
                bot.GetAttachableInDirectionOf<AttachableBase>(from, DIRECTION.DOWN)
            };

            return check
                .Where(ab => ab != null)
                .Select(ab => ab.Coordinate)
                .ToList();

        }
        
        /// <summary>
        /// Returns an AttachableBase in the specified direction from the target Attachable
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttachableInDirectionOf<T>(this Bot bot, AttachableBase from, DIRECTION direction) where T: AttachableBase
        {
            var coord = from.Coordinate + direction.ToVector2Int();

            return bot.attachedBlocks.FirstOrDefault(a => a.Coordinate == coord) as T;
        }
        
        #endregion //Obtaining Bits
        
        //============================================================================================================//
    }
}

