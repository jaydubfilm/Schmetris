using StarSalvager.Editor.PatchTrees.Graph;
using StarSalvager.ScriptableObjects.PatchTrees;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.Editor.PatchTrees
{
    [CustomEditor(typeof(PatchTreeContainer))]
    public class PatchTreeContainerCustomEditor : UnityEditor.Editor
    {
        private PatchTreeContainer _patchTreeContainer;

        private void OnEnable()
        {
            _patchTreeContainer = (PatchTreeContainer) target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open in Patch Tree Editor"))
            {
                OpenPatchTreeEditor();
            }
        }

        private void OpenPatchTreeEditor()
        {
            PatchTreeWindow.CreatePatchTreeWindow(_patchTreeContainer);
        }
    }
}
