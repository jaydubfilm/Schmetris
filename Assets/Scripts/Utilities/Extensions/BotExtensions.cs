using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.Utilities;
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
                .Where(b => b is ISaveable )
                .ForEach(b => (b as ISaveable)?.ToBlockData())
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
                IAttachable attachable;
                switch (block.ClassType)
                {
                    case nameof(Bit):
                        attachable = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<IAttachable>(block);
                        break;
                    case nameof(Part):
                        attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<IAttachable>(block);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(block.ClassType), block.ClassType, null);
                }
                
                bot.AttachNewBit(attachable.Coordinate, attachable);
            }
            
        }
        
        #endregion //Import/Export
        
        //============================================================================================================//
        
        

        
        //============================================================================================================//
        
        /// <summary>
        /// Fill ref List with all Bits of similar level & type in specified direction.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="target"></param>
        /// <param name="direction"></param>
        /// <param name="bitList"></param>
        public static void ComboCount(this Bot bot, Bit target, DIRECTION direction, ref List<Bit> bitList)
        {
            bot.attachedBlocks.ComboCountAlgorithm(target.Type, target.level, target.Coordinate, direction.ToVector2Int(),
                ref bitList);
        }

    }
}

