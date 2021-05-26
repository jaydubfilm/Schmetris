using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.PatchTrees
{    
    [Serializable]
    public abstract class BaseNodeData
    {
        public virtual string ClassType { get; }
        public string GUID;
        public Rect Position;

        public int Type;
    }
    [Serializable]
    public class PatchNodeData : BaseNodeData
    {
        public override string ClassType => nameof(PATCH_TYPE);
        public int Tier;
        public int Level;
    }
    [Serializable]
    public class PartNodeData : BaseNodeData
    {
        public override string ClassType => nameof(PART_TYPE);
    }
    
    
    [Serializable]
    public class PatchTreeContainer : ScriptableObject
    {
        public PART_TYPE PartType;
        
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public PartNodeData PartNodeData;
        public List<PatchNodeData> PatchNodeDatas = new List<PatchNodeData>();
    }
    
    [Serializable]
    public class NodeLinkData 
    {
        public string BaseNodeGUID;
        public string PortName;
        public string TargetNodeGUID;
    }
}
