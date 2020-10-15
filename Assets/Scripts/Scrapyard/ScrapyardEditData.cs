using System.Collections;
using System.Collections.Generic;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public struct ScrapyardEditData
    {
        public SCRAPYARD_ACTION EventType;
        public Vector2Int Destination;

        public BlockData BlockData;
        public float Value;

        /*public Vector2Int Coordinate;

        public PART_TYPE PartType;
        public BIT_TYPE BitType;
        public int Level;*/
    }
}