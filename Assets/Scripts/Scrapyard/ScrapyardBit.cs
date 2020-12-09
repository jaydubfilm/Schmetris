﻿using Sirenix.OdinInspector;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager
{
    public class ScrapyardBit : MonoBehaviour, IAttachable, IBit, ISaveable
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
        
        //IAttachable properties
        //============================================================================================================//

        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool CountAsConnectedToCore => true;
        public bool CanDisconnect => true;

        [ShowInInspector, ReadOnly]
        public bool CanShift => true;

        public bool CountTowardsMagnetism => true;

        //Bit Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public BIT_TYPE Type { get; set; }
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        [SerializeField]
        private LayerMask collisionMask;

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
            Type = (BIT_TYPE)blockData.Type;
            level = blockData.Level;
        }

        //============================================================================================================//

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }
        
        //IHasBounds Functions
        //====================================================================================================================//
        
        public Bounds GetBounds()
        {
            return new Bounds
            {
                center = transform.position,
                size = Vector2.one * Constants.gridCellSize
            };
        }

        //====================================================================================================================//
    }
}