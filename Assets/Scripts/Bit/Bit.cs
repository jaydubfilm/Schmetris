using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Bit : AttachableBase, IBit
    {
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

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        protected override void OnCollide()
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
                Level = level
            };
        }

        public override void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (BIT_TYPE) blockData.Type;
            level = blockData.Level;
        }
    }
}