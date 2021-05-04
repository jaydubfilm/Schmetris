using UnityEditor;
using UnityEditor.UIElements;
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
            GenerateToolbar();
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
        
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var fileNameTextField = new TextField("File Name:");
            fileNameTextField.SetValueWithoutNotify(_fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(fileNameTextField);

            toolbar.Add(new Button(() => _graphView.CreateNewPatchNode("Dialogue Node")) {text = "New Node",});
            toolbar.Add(new Button/*(() => RequestDataOperation(true))*/ {text = "Save Data"});

            toolbar.Add(new Button/*(() => RequestDataOperation(false))*/ {text = "Load Data"});
            // toolbar.Add(new Button(() => _graphView.CreateNewDialogueNode("Dialogue Node")) {text = "New Node",});
            rootVisualElement.Add(toolbar);
        }

        //====================================================================================================================//
        
    }
}
