
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.PatchTrees;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace StarSalvager.Editor.PatchTrees.Nodes
{
    
       
    public class PartNode : BaseNode
    {
        [OnValueChanged("Updated")]
        public PART_TYPE PartType;
        
        public override BaseNodeData GetNodeData()
        {
            return new PartNodeData
            {
                GUID = GUID,
                Type = (int) PartType,
                Position = GetPosition()
            };
        }

        public override void LoadFromNodeData(in BaseNodeData nodeData)
        {
            if (!(nodeData is PartNodeData partNodeData))
                throw new Exception();

            GUID = partNodeData.GUID;
            PartType = (PART_TYPE)partNodeData.Type;
            SetPosition(nodeData.Position);
        }

#if UNITY_EDITOR
        public void Updated()
        {
            title = PartType.ToString();
        }
#endif
    }
}
