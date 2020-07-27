using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class MissionDataExtensions
    {
        public static List<Mission> ImportBlockDatas(this List<BlockData> blockDatas, bool inScrapyardForm)
        {
            List<IAttachable> attachables = new List<IAttachable>();

            foreach (BlockData blockData in blockDatas)
            {
                switch (blockData.ClassType)
                {
                    case "Bit":
                    case "ScrapyardBit":
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<ScrapyardBit>(blockData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(blockData));
                        }
                        break;
                    case "Part":
                    case "ScrapyardPart":
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(blockData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<Part>(blockData));
                        }
                        break;
                }
            }

            return attachables;
        }
    }
}