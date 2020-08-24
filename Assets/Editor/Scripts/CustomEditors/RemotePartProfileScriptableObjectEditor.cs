using Sirenix.OdinInspector.Editor;
using StarSalvager.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.Editor.CustomEditors
{
    [CustomEditor(typeof(RemotePartProfileScriptableObject))]
    public class RemotePartProfileScriptableObjectEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Bulk Editor"))
            {
                PartCostBulkEditor.BulkPartCostEditor();
            }
        
            base.OnInspectorGUI();
        }
    }
}
