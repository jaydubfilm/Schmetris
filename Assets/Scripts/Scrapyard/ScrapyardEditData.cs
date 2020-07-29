using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public struct ScrapyardEditData
    {
        public SCRAPYARD_ACTION EventType;
        public Vector2Int Coordinate;

        public PART_TYPE PartType;
        public BIT_TYPE BitType;
        public int Level;
    }
}