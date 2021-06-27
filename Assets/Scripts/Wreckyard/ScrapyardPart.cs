﻿using System;
using System.Collections.Generic;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager
{
    [Obsolete]
    public class ScrapyardPart : Actor2DBase, IAttachable, ISaveable<PartData>, IPart, ICustomRecycle
    {

        //IAttachable Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool CountAsConnectedToCore => true;
        public bool CanDisconnect => false;

        [ShowInInspector, ReadOnly]
        public bool CanShift => false;

        public bool CountTowardsMagnetism => false;

        //Part Properties
        //============================================================================================================//

        public bool Disabled => false;

        [ShowInInspector, ReadOnly]
        public PART_TYPE Type { get; set; }

        public List<PatchData> Patches { get; set; }

        //IPart Functions
        //====================================================================================================================//
        
        public void AddPatch(in PatchData patchData)
        {
            for (int i = 0; i < Patches.Count; i++)
            {
                if(Patches[i].Type != (int)PATCH_TYPE.EMPTY)
                    continue;

                Patches[i] = patchData;
                return;
            }

            throw new Exception("No available space for new patch");
        }

        public void RemovePatch(in PatchData patchData)
        {
            for (int i = 0; i < Patches.Count; i++)
            {
                if(!Patches[i].Equals(patchData))
                    continue;

                Patches[i] = default;
                
                return;
            }

            throw new Exception($"No Patch found matching {(PATCH_TYPE)patchData.Type}[{patchData.Level}]");
        }

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
        }

        //ISaveable Functions
        //============================================================================================================//

        public PartData ToBlockData()
        {
            return new PartData
            {
                //ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int)Type,
                Patches =  Patches
            };
        }

        public void LoadBlockData(IBlockData blockData)
        {
            throw new System.NotImplementedException();
        }

        public void LoadBlockData(PartData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (PART_TYPE)blockData.Type;
            Patches = blockData.Patches;
        }

        //ICustomRecycle Functions
        //====================================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {

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
        IBlockData ISaveable.ToBlockData()
        {
            return ToBlockData();
        }
    }
}