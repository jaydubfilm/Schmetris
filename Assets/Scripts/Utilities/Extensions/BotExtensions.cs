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

            var data = bot.attachedBlocks.OfType<ISaveable>().Select(x => x.ToBlockData())
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
                
                bot.AttachNewBlock(attachable.Coordinate, attachable);
            }
            
        }
        
        #endregion //Import/Export
        
        //============================================================================================================//
        
        public static List<BlockData> GetBlockDatas(this Bot bot)
        {
            var blockDatas = new List<BlockData>();
            
            var attachables = new List<IAttachable>(bot.attachedBlocks);
            //var ignoreAttachables = bot.PendingDetach == null
            //    ? new List<IAttachable>()
            //    : new List<IAttachable>(bot.PendingDetach);

            foreach (var attachable in attachables.Where(attachable => !(attachable is ICanDetach iCanDetach && iCanDetach.PendingDetach)))
            {
                if (attachable is ISaveable saveable)
                {
                    blockDatas.Add(saveable.ToBlockData());
                }
            }

            return blockDatas;
        }

        public static List<BlockData> GetBlockDatas(this ScrapyardBot bot)
        {
            var blockDatas = new List<BlockData>();

            var attachables = new List<IAttachable>(bot.attachedBlocks);

            foreach (var attachable in attachables)
            {
                if (attachable is ISaveable saveable)
                {
                    blockDatas.Add(saveable.ToBlockData());
                }
            }

            return blockDatas;
        }


        //============================================================================================================//

        /// <summary>
        /// Fill ref List with all Bits of similar level & type in specified direction.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="target"></param>
        /// <param name="direction"></param>
        /// <param name="iCanCombos"></param>
        public static void ComboCount<T>(this Bot bot, ICanCombo<T> target, DIRECTION direction, ref List<ICanCombo> iCanCombos) where T: Enum
        {
            var combo = bot.attachedBlocks.OfType<ICanCombo>();
            combo.ComboCountAlgorithm(target.Type, target.level, target.Coordinate, direction.ToVector2Int(),
                ref iCanCombos);
        }


        //====================================================================================================================//

        public static void SetColliderActive(this Bot bot, bool state)
        {
            foreach (var collidableBase in bot.attachedBlocks.OfType<CollidableBase>())
            {
                collidableBase.SetColliderActive(state);
            }
        }

    }
}

