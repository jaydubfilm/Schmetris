using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.PatchTrees;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace StarSalvager.Editor.PatchTrees.Nodes
{
    
    
    public abstract class BaseNode : Node
    {
        public string GUID { get; set; }

        public abstract BaseNodeData GetNodeData();
        public abstract void LoadFromNodeData(in BaseNodeData nodeData);

    }
}
