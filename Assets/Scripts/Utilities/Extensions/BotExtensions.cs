using System;
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
        
        //FIXME I don't like that these are here, I want them not in the Bot, but also not here
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
        
        public static void GetAllAttachedBits<T>(this Bot bot, AttachableBase current, AttachableBase[] toIgnore, ref List<T> bits) where T: AttachableBase
        {
            var bitsAround = bot.GetAttachablesAround<T>(current);

            bits.Add(current as T);
            
            foreach (var bit in bitsAround)
            {
                if (bit == null)
                    continue;

                if (toIgnore != null && toIgnore.Contains(bit))
                    continue;
                
                if(bits.Contains(bit))
                    continue;

                bot.GetAllAttachedBits(bit, toIgnore, ref bits);
            }

        }
        
        #endregion //Obtaining Bits
        
        //============================================================================================================//
        
        #region Path to Core Checks
        
        /// <summary>
        /// Returns whether or not this AttachableBase has a clear path to the core.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="checking"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        public static bool HasPathToCore(this Bot bot, AttachableBase checking, List<Vector2Int> toIgnore = null)
        {
            var travelled = new List<Vector2Int>();
            //Debug.LogError("STARTED TO CHECK HERE");
            return bot.PathAlgorithm(checking, toIgnore, ref travelled);
        }
        
        private static bool PathAlgorithm(this Bot bot, AttachableBase current, ICollection<Vector2Int> toIgnore, ref List<Vector2Int> travelled)
        {
            //If we're on (0, 0) we've reached the core, so go back up through 
            if (current.Coordinate == Vector2Int.zero)
                return true;

            //Get list of attachables around the current attachable
            var attachablesAround = bot.GetAttachablesAround<AttachableBase>(current);
            
            for (var i = 0; i < attachablesAround.Count; i++)
            {
                //If there's no attachable, keep going
                if (attachablesAround[i] == null)
                    continue;

                // If ignore list contains this Coordinate, keep going
                if (toIgnore != null && toIgnore.Contains(attachablesAround[i].Coordinate))
                {
                    //Debug.LogError($"toIgnore contains {around[i].Coordinate}");
                    attachablesAround[i] = null;
                    continue;
                }

                // If we've not already been at this Coordinate, keep going
                if (!travelled.Contains(attachablesAround[i].Coordinate))
                    continue;

                //Debug.LogError($"travelled already contains {around[i].Coordinate}");
                attachablesAround[i] = null;
            }

            //Check to see if the list is completely null
            if (attachablesAround.All(ab => ab == null))
            {
                //Debug.LogError($"FAILED. Nothing around {current}", current);
                return false;
            }

            //If everything checks out, lets say we've been here
            travelled.Add(current.Coordinate);

            //Get a list of all non-null Attachables ordered by the shortest distance to the core
            var closestAttachables = attachablesAround.Where(ab => ab != null)
                .OrderBy(ab => Vector2Int.Distance(Vector2Int.zero, ab.Coordinate));
            
            
            var result = false;
            //Go through all of the attachables (Closest to Furthest) until we run out
            foreach (var attachableBase in closestAttachables)
            {
                result = bot.PathAlgorithm(attachableBase, toIgnore, ref travelled);

                //Debug.LogError($"{result} when checking {current.gameObject.name} to {attachableBase.gameObject.name}");

                //If something reached the core, just stop looping and let the system know
                if (result)
                    break;
            }

            //Debug.LogError($"Failed Totally at {current.Coordinate}", current);
            return result;
        }
        
        
        #endregion //Path to Core Checks
        
        //============================================================================================================//

    }
}

