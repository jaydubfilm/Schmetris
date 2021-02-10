using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager
{
    public class Part : CollidableBase, IAttachable, ICustomRotate, ISaveable<PartData>, IPart, ICustomRecycle
    {
        //IAttachable Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }

        public bool Attached => true;

        public bool CountAsConnectedToCore => true;
        public bool CanShift => false;
        public bool CountTowardsMagnetism => false;

        //Part Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public PART_TYPE Type { get; set; }

        public PatchData[] Patches { get; set; }


        public bool LockRotation { get; set; }

        public Color partColor = Color.white;

        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                SetColor(value ? Color.gray : partColor);
            }
        }

        private bool _disabled;

        //Unity Functions
        //====================================================================================================================//
        
        private void Start()
        {
            collider.usedByComposite = true;
        }

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
        }

        //Part Functions
        //============================================================================================================//
        
        public void AddPatch(in PatchData patchData)
        {
            for (int i = 0; i < Patches.Length; i++)
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
            for (int i = 0; i < Patches.Length; i++)
            {
                if(!Patches[i].Equals(patchData))
                    continue;

                Patches[i] = default;
                
                return;
            }

            throw new Exception($"No Patch found matching {(PATCH_TYPE)patchData.Type}[{patchData.Level}]");
        }

        protected override void OnCollide(GameObject gObj, Vector2 worldHitPoint)
        {
#if !UNITY_EDITOR
            //FIXME Need to find the cause of parts not despawning correctly
            if (IsRecycled)
                return;
            
            Recycler.Recycle<Part>(this);
#else
            throw new Exception("PARTS SHOULD NOT COLLIDE");
#endif

        }

        //ICustomRotateFunctions
        //====================================================================================================================//
        
        public void CustomRotate(Quaternion rotation)
        {
            if (LockRotation)
                return;
            
            transform.localRotation = rotation;
        }
        
        //ISaveable Functions
        //============================================================================================================//

        public PartData ToBlockData()
        {
            return new PartData
            {
                Coordinate = Coordinate,
                Type = (int) Type,
                Patches = Patches
            };
        }

        public void LoadBlockData(IBlockData blockData)
        {
            throw new NotImplementedException();
        }

        public void LoadBlockData(PartData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (PART_TYPE) blockData.Type;
            Patches = blockData.Patches;
        }

        //============================================================================================================//


        public void CustomRecycle(params object[] args)
        {
            SetSortingLayer(LayerHelper.ACTORS);
            
            SetColor(Color.white);

            Disabled = false;
            SetColliderActive(true);
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
