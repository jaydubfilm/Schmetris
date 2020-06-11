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
                        attachable = AttachableFactory.Instance.GetFactory<BitAttachableFactory>().CreateObject<AttachableBase>(block);
                        break;
                    case nameof(Part):
                        attachable = AttachableFactory.Instance.GetFactory<PartAttachableFactory>().CreateObject<AttachableBase>(block);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(block.ClassType), block.ClassType, null);
                }
                
                bot.AttachNewBit(attachable.Coordinate, attachable);
            }
            
        }
    }
}

