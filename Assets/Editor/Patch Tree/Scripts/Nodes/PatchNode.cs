using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.PatchTrees;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace StarSalvager.Editor.PatchTrees.Nodes
{


    public class PatchNode : BaseNode
    {
        public PATCH_TYPE PatchType;
        [UnityEngine.Range(1, 4)] public int Tier;

        public override BaseNodeData GetNodeData()
        {
            return new PatchNodeData
            {
                GUID = GUID,
                Type = (int) PatchType,
                Tier = Tier,
                Position = GetPosition()
            };
        }

        public override void LoadFromNodeData(in BaseNodeData nodeData)
        {
            if (!(nodeData is PatchNodeData patchNodeData))
                throw new Exception();

            GUID = patchNodeData.GUID;
            PatchType = (PATCH_TYPE)patchNodeData.Type;
            Tier = patchNodeData.Tier;
            SetPosition(nodeData.Position);
        }
    }
}
