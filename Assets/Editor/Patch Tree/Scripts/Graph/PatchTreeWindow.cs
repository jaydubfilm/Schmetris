using System;
using StarSalvager.Factories;
using StarSalvager.PatchTrees;
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

        private static PatchTreeContainer patchTreeContainerToLoad;
        
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
        
        [MenuItem("Graph/Patch Tree Graph")]
        public static void CreatePatchTreeWindow()
        {
            var window = GetWindow<PatchTreeWindow>();
            window.titleContent = new GUIContent(WINDOW_NAME);
        }
        public static void CreatePatchTreeWindow(in PatchTreeContainer patchTreeContainer)
        {
            patchTreeContainerToLoad = patchTreeContainer;

            //partTypeToLoad = partType;
            var window = GetWindow<PatchTreeWindow>();
            window.titleContent = new GUIContent(WINDOW_NAME);
        }
        
        private void ConstructGraphView()
        {
            if (patchTreeContainerToLoad is null)
                throw new ArgumentException();

            var partType = patchTreeContainerToLoad.PartType;
            
            _graphView = new PatchTreeGraphView(this, partType)
            {
                name = WINDOW_NAME,
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
            
            PatchTreeSaveUtility.LoadPatchTree(partType, _graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            /*var fileNameTextField = new TextField("File Name:");
            fileNameTextField.SetValueWithoutNotify(_fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(fileNameTextField);*/

            toolbar.Add(new Button(() => _graphView.CreateNewPatchNode("Patch Node")) {text = "New Node",});
            toolbar.Add(new Button(
                () =>
            {
                PatchTreeSaveUtility.SaveGraph(patchTreeContainerToLoad.PartType, _graphView); 
            })
            {
                text = "Save Data"
            });

            //toolbar.Add(new Button(() => RequestDataOperation(false)) {text = "Load Data"});
            // toolbar.Add(new Button(() => _graphView.CreateNewDialogueNode("Dialogue Node")) {text = "New Node",});
            rootVisualElement.Add(toolbar);
        }
        
        /*private void RequestDataOperation(bool save)
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                //var saveUtility = PatchTreeSaveUtility.GetInstance(_graphView);
                if (save) PatchTreeSaveUtility.SaveGraph(_fileName, _graphView);
                else PatchTreeSaveUtility.LoadPatchTree(_fileName, _graphView);
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid File name", "Please Enter a valid filename", "OK");
            }
        }*/

        //====================================================================================================================//
        
    }
}
