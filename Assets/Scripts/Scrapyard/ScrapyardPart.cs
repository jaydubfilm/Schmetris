using Sirenix.OdinInspector;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class ScrapyardPart : MonoBehaviour, IAttachable, ISaveable, IPart
    {
        protected new SpriteRenderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = gameObject.GetComponent<SpriteRenderer>();

                return _renderer;
            }
        }
        private SpriteRenderer _renderer;


        public new Transform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.GetComponent<Transform>();

                return _transform;
            }
        }
        private Transform _transform;

        //IAttachable Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool CountAsConnected => true;
        public bool CanDisconnect => false;

        [ShowInInspector, ReadOnly]
        public bool CanShift => false;

        //Part Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public PART_TYPE Type { get; set; }
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
        }

        //ISaveable Functions
        //============================================================================================================//

        public BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = level
            };
        }

        public void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (PART_TYPE)blockData.Type;
            level = blockData.Level;
        }

        //============================================================================================================//

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        public void SetLevel(int newLevel)
        {
            level = newLevel;
        }
    }
}