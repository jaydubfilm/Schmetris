using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Part : AttachableBase, IPart
    {
        //============================================================================================================//

        public PART_TYPE Type
        {
            get => _type;
            set => _type = value;
        }

        private PART_TYPE _type;
        
        public int level { get => _level; set => _level = value; }
        [SerializeField]
        private int _level;
        
        //============================================================================================================//
        
        

        //============================================================================================================//

        protected override void OnCollide(GameObject gObj, Vector2 hitPoint)
        {
            throw new System.NotImplementedException();
        }

        public override BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int) Type,
                Level = level
            };
        }

        public override void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (PART_TYPE) blockData.Type;
            level = blockData.Level;
        }

        //============================================================================================================//
    }
}

