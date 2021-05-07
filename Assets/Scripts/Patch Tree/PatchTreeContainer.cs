using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.PatchTrees
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
    }
    [Serializable]
    public class PartNodeData : BaseNodeData
    {
        public override string ClassType => nameof(PART_TYPE);
    }
    
    
    [Serializable]
    public class PatchTreeContainer : ScriptableObject
    {
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public PartNodeData PartNodeData;
        public List<PatchNodeData> PatchNodeDatas = new List<PatchNodeData>();
        //public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
        //public List<CommentBlockData> CommentBlockData = new List<CommentBlockData>();
    }
    
    [Serializable]
    public class NodeLinkData 
    {
        public string BaseNodeGUID;
        public string PortName;
        public string TargetNodeGUID;
    }
    
    /*[Serializable]
    public class DialogueNodeData
    {
        public string NodeGUID;
        public string DialogueText;
        public Vector2 Position;
    }*/
    
    /*[System.Serializable]
    public class ExposedProperty
    {
        public static ExposedProperty CreateInstance()
        {
            return new ExposedProperty();
        }

        public string PropertyName = "New String";
        public string PropertyValue = "New Value";
    }*/
}
