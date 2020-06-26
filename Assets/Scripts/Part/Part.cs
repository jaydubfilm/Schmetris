using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Part : CollidableBase, IAttachable, ISaveable, IPart, IHealth
    {
        public Vector2Int Coordinate { get; set; }
        public bool Attached { get; set; }
        
        public float StartingHealth { get; }
        public float CurrentHealth { get; }

        
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
        
        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }
        
        public void ChangeHealth(float amount)
        {
            //throw new System.NotImplementedException();
        }

        //============================================================================================================//

        protected override void OnCollide(GameObject gObj, Vector2 hitPoint)
        {
            throw new System.NotImplementedException();
        }

        public BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int) Type,
                Level = level
            };
        }

        public void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (PART_TYPE) blockData.Type;
            level = blockData.Level;
        }

        //============================================================================================================//



    }
}

