using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Part : AttachableBase, IPart
    {
        public PART_TYPE Type { get =>_type; set => _type = value; }
        private PART_TYPE _type;

        protected override void OnCollide(GameObject _)
        {
            throw new System.NotImplementedException();
        }

        public override BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = -1
            };
        }
        
        public override void LoadBlockData(BlockData blockData)
        {
            Coordinate = (Vector2Int) blockData.Coordinate;
            Type = (PART_TYPE) blockData.Type;
        }
    }
}

