using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    public class ScrapyardLayout
    {
        public string Name;
        public List<BlockData> BlockData;

        public ScrapyardLayout(string name, List<BlockData> blockData)
        {
            Name = name;
            BlockData = blockData;
        }
    }
}