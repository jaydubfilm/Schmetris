﻿using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Part : CollidableBase, IAttachable, ISaveable, IPart, IHealth
    {
        //IAttachable Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }
        [ShowInInspector, ReadOnly]
        public bool CanShift => false;
        
        //IHealth Properties
        //============================================================================================================//
        
        public float StartingHealth { get; }
        public float CurrentHealth { get; }

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
            collider.usedByComposite = isAttached;
        }

        //IHealth Functions
        //============================================================================================================//
        
        public void ChangeHealth(float amount)
        {
            //throw new System.NotImplementedException();
        }

        //Part Functions
        //============================================================================================================//

        protected override void OnCollide(GameObject gObj, Vector2 hitPoint)
        {
            throw new System.NotImplementedException();
        }

        //ISaveable Functions
        //============================================================================================================//
        
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
