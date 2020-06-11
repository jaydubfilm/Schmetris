using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Bit : AttachableBase, IBit
    {
        //============================================================================================================//
        
        public BIT_TYPE Type
        {
            get => _type;
            set => _type = value;
        }
        [SerializeField]
        private BIT_TYPE _type;
        public int level { get => _level; set => _level = value; }
        [SerializeField]
        private int _level;
        
        //============================================================================================================//

        protected override void OnCollide(Bot bot)
        {
            bot.TryAddNewAttachable(this);
        }
        
        //============================================================================================================//

        public override BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = level
            };
        }

        public override void LoadBlockData(BlockData blockData)
        {
            //FIXME Might want to consider BlockData that has Coordinate of (0, 0) or null
            Coordinate = blockData.Coordinate;
            Type = (BIT_TYPE) blockData.Type;
            level = blockData.Level;
        }
        
        //============================================================================================================//

    }
}