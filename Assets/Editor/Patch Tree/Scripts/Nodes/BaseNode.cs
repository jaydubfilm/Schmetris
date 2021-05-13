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

    }
    public abstract class BaseNode<T> : BaseNode where T: BaseNodeData
    {
        public abstract T GetNodeData();
        public abstract void LoadFromNodeData(in T nodeData);

    }
}
