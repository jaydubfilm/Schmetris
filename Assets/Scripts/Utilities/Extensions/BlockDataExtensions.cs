using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class BlockDataExtensions
    {
        public static List<IAttachable> ImportBlockDatas(this List<BlockData> blockDatas, bool inScrapyardForm)
        {
            List<IAttachable> attachables = new List<IAttachable>();

            foreach (BlockData blockData in blockDatas)
            {
                switch (blockData.ClassType)
                {
                    case nameof(Bit):
                    case nameof(ScrapyardBit):
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<ScrapyardBit>(blockData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(blockData));
                        }
                        break;
                    case nameof(Component):
                        //case nameof(ScrapyardBit):
                        var component = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>()
                            .CreateObject<Component>(blockData);
                            attachables.Add(component);
                            /*if (inScrapyardForm)
                            {
                                throw new NotImplementedException();
                            }
                            else
                            {
                                
                            }*/
                        break;
                    case nameof(Part):
                    case nameof(ScrapyardPart):
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(blockData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<Part>(blockData));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(blockData.ClassType), blockData.ClassType, null);
                }
            }

            return attachables;
        }
    }
}