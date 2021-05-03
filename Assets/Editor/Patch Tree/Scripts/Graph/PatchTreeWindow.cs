using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//Based on: https://github.com/merpheus-dev/NodeBasedDialogueSystem/blob/master/com.subtegral.dialoguesystem/Editor/Graph/StoryGraph.cs
namespace StarSalvager.Editor.PatchTrees.Graph
{
    public class PatchTreeWindow : EditorWindow
    {
        private const string WINDOW_NAME = "Patch Tree Graph";
        private string _fileName = "New Patch Tree";
        
        private PatchTreeGraphView _graphView;

        //Unity Functions
        //====================================================================================================================//
        
        private void OnEnable()
        {
            ConstructGraphView();
        }
        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        //Graph Functions
        //====================================================================================================================//
        
        [MenuItem("Graph/Narrative Graph")]
        public static void CreatePatchTreeWindow()
        {
            var window = GetWindow<PatchTreeWindow>();
            window.titleContent = new GUIContent(WINDOW_NAME);
        }
        
        private void ConstructGraphView()
        {
            _graphView = new PatchTreeGraphView(this)
            {
                name = WINDOW_NAME,
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        //====================================================================================================================//
        
    }
}
