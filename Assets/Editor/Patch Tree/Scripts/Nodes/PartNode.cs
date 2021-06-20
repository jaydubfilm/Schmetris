
using System;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects.PatchTrees;

namespace StarSalvager.Editor.PatchTrees.Nodes
{
    public class PartNode : BaseNode<PartNodeData>
    {
        [OnValueChanged("UpdateTitle")]
        public PART_TYPE PartType;
        
        public override PartNodeData GetNodeData()
        {
            return new PartNodeData
            {
                GUID = GUID,
                Type = (int) PartType,
                Position = GetPosition()
            };
        }

        public override void LoadFromNodeData(in PartNodeData nodeData)
        {
            if (!(nodeData is PartNodeData partNodeData))
                throw new Exception();

            GUID = partNodeData.GUID;
            PartType = (PART_TYPE)partNodeData.Type;
            SetPosition(nodeData.Position);

            UpdateTitle();
        }

#if UNITY_EDITOR
        public override void UpdateTitle()
        {
            title = PartType.ToString();
        }
#endif
    }
}
