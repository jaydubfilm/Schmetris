
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace StarSalvager.Editor.PatchTrees.Nodes
{
    public class PartNode : Node
    {
        public string GUID;

        [OnValueChanged("Updated")]
        public PART_TYPE PartType;

#if UNITY_EDITOR
        public void Updated()
        {
            title = PartType.ToString();
        }
#endif
    }
}
