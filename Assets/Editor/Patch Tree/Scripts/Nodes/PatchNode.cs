using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace StarSalvager.Editor.PatchTrees.Nodes
{
    public class PatchNode : Node
    {
        public string GUID;

        public PATCH_TYPE PatchType;
        [UnityEngine.Range(1,4)]
        public int Tier;
    }
}
