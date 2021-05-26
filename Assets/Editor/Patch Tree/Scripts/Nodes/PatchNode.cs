using System;
using StarSalvager.ScriptableObjects.PatchTrees;
using UnityEngine;

namespace StarSalvager.Editor.PatchTrees.Nodes
{


    public class PatchNode : BaseNode<PatchNodeData>
    {
        public PATCH_TYPE PatchType;
        [UnityEngine.Range(1, 4)] public int Tier;
        [UnityEngine.Range(1, 4)] public int Level;

        public override PatchNodeData GetNodeData()
        {
            return new PatchNodeData
            {
                GUID = GUID,
                Type = (int) PatchType,
                Tier = Tier,
                Level = Level,
                Position = GetPosition()
            };
        }

        public override void LoadFromNodeData(in PatchNodeData nodeData)
        {
            if (!(nodeData is PatchNodeData patchNodeData))
                throw new Exception();

            GUID = patchNodeData.GUID;
            PatchType = (PATCH_TYPE)patchNodeData.Type;
            Tier = patchNodeData.Tier;
            Level = patchNodeData.Level;
            SetPosition(nodeData.Position);
            
            UpdateTitle();
        }

        public override void UpdateTitle()
        {
            title = $"{PatchType} {Level}";
        }
        
        public void UpdatePosition()
        {
            SetPosition(GetPosition());
        }
        public override void SetPosition(Rect newPos)
        {
            newPos.x = Tier * 250;
            
            base.SetPosition(newPos);
        }
    }
}
