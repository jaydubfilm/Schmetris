using System;
using System.Collections.Generic;
using StarSalvager.Editor.PatchTrees.Nodes;
using StarSalvager.PatchTrees;
using StarSalvager.ScriptableObjects.PatchTrees;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StarSalvager.Editor.PatchTrees.Graph
{
    //Based on: https://github.com/merpheus-dev/NodeBasedDialogueSystem/blob/master/com.subtegral.dialoguesystem/Editor/Graph/StoryGraphView.cs
    public class PatchTreeGraphView : GraphView
    {
        //Properties
        //====================================================================================================================//
        
        public readonly Vector2 DefaultNodeSize = new Vector2(200, 150);

        public readonly PART_TYPE PartType;

        //Constructor
        //====================================================================================================================//

        #region Constructor

        public PatchTreeGraphView(PatchTreeWindow editorWindow, in PART_TYPE partType)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("PatchTreeGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            var partNode = GetEntryPointNodeInstance(partType);
            PartType = partType;
            
            AddElement(partNode);
        }

        #endregion //Constructor
        
        //Creating Nodes
        //====================================================================================================================//

        #region Creating Nodes
        
        public void CreateNewPatchNode(string nodeName)
        {
            AddElement(CreateNode(nodeName, new Vector2(100, 200), 
                new PatchNodeData
                {
                    GUID = Guid.NewGuid().ToString(),
                    Type = (int)PATCH_TYPE.EMPTY,
                    Tier = 1,
                    Level = 1
                }));
        }
        public void CreateNewPatchNode(string nodeName, Vector2 position)
        {
            AddElement(CreateNode(nodeName, position, 
                new PatchNodeData
                {
                    GUID = Guid.NewGuid().ToString(),
                    Type = (int)PATCH_TYPE.EMPTY,
                    Tier = 1,
                    Level = 1
                })
            );
        }
        
        public Node CreateNode(string nodeName, Vector2 position, BaseNodeData nodeData)
        {
            return CreateNode(nodeName, new Rect(position, DefaultNodeSize), nodeData);
        }
        public Node CreateNode(string nodeName, Rect nodeRect, BaseNodeData nodeData)
        {
            BaseNode tempNode;

            switch (nodeData)
            {
                case PartNodeData partNodeData:
                    tempNode = new PartNode();
                    ((PartNode)tempNode).LoadFromNodeData(partNodeData);
                    break;
                case PatchNodeData patchNodeData:
                    tempNode = new PatchNode();
                    ((PatchNode)tempNode).LoadFromNodeData(patchNodeData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //--------------------------------------------------------------------------------------------------------//

            if (!(tempNode is PatchNode patchNode))
                return tempNode;
            
            tempNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            
            var inputPort = GetPortInstance(tempNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Pre Req";
            var outputPort = GetPortInstance(tempNode, Direction.Output, Port.Capacity.Multi);
            outputPort.portName = "Unlocks";
            tempNode.inputContainer.Add(inputPort);
            tempNode.outputContainer.Add(outputPort);
            
            tempNode.RefreshExpandedState();
            tempNode.RefreshPorts();
            tempNode.SetPosition(nodeRect); //To-Do: implement screen center instantiation positioning

            //--------------------------------------------------------------------------------------------------------//
            var enumField = CreateEnumField("Patch Type", patchNode.PatchType, PATCH_TYPE.EMPTY, type =>
            {
                patchNode.PatchType = type;
                patchNode.UpdateTitle();
            });
            tempNode.mainContainer.Add(enumField);

            
            tempNode.mainContainer.Add(CreateIntInputField("Tier",patchNode.Tier, 1 , 4, tier =>
            {
                patchNode.Tier = tier;
                patchNode.UpdatePosition();
            }));
            tempNode.mainContainer.Add(CreateSliderField("Level",patchNode.Level, 1 , 4, level =>
            {
                patchNode.Level = level;
                patchNode.UpdateTitle();
            }));
            
            patchNode.UpdateTitle();

            return tempNode;
        }

        #endregion //Creating Nodes

        //Ports
        //====================================================================================================================//

        #region Ports

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var startPortView = startPort;

            ports.ForEach(port =>
            {
                if (startPortView != port && startPortView.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        
        private Port GetPortInstance(Node node, Direction nodeDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }
        
        //TODO Need to finish integrating this
        private PartNode GetEntryPointNodeInstance(in PART_TYPE partType)
        {
            var partNode = new PartNode
            {
                //title = "START",
                GUID = Guid.NewGuid().ToString(),
                PartType = partType
                //DialogueText = "ENTRYPOINT",
                //EntyPoint = true
            };
            partNode.UpdateTitle();

            var generatedPort = GetPortInstance(partNode, Direction.Output, Port.Capacity.Multi);
            generatedPort.portName = "Patches";
            partNode.outputContainer.Add(generatedPort);

            partNode.capabilities &= ~Capabilities.Movable;
            partNode.capabilities &= ~Capabilities.Deletable;

            partNode.RefreshExpandedState();
            partNode.RefreshPorts();
            partNode.SetPosition(new Rect(100, 200, 100, 150));
            return partNode;
        }

        #endregion //Ports

        //Creating Visual Elements
        //====================================================================================================================//

        #region Creating Visual Elements

        private static VisualElement CreateEnumField<T>(in string title, in T value, in T @default, Action<T> onValueChanged) where T : Enum
        {
            var enumField = new EnumField(title, @default);
            enumField.styleSheets.Add(Resources.Load<StyleSheet>("EnumField"));
            enumField.RegisterValueChangedCallback(evt =>
            {
                onValueChanged?.Invoke((T)evt.newValue);
            });
            enumField.SetValueWithoutNotify(value);

            return enumField;
        }
        
        private static VisualElement CreateSliderField(in string title, in int value, in int min, in int max, Action<int> onValueChangedCallback)
        {
            var layoutTest = new Box();
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("HorizontalLayout"));
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("Sliders"));
            layoutTest.AddToClassList("flex-horizontal");

            
            var intSlider = new SliderInt(title, min, max);
            
            var intField = new IntegerField("");
            layoutTest.contentContainer.Add(intSlider);
            layoutTest.contentContainer.Add(intField);
            
            intSlider.RegisterValueChangedCallback(evt =>
            {
                intField.SetValueWithoutNotify(evt.newValue);
                onValueChangedCallback?.Invoke(evt.newValue);
            });
            
            var tempMin = min;
            var tempMax = max;
            intField.RegisterValueChangedCallback(evt =>
            {
                var newValue = Mathf.Clamp(evt.newValue, tempMin, tempMax);

                intSlider.SetValueWithoutNotify(newValue);
                onValueChangedCallback?.Invoke(newValue);   
            });
            
            intSlider.SetValueWithoutNotify(value);
            intField.SetValueWithoutNotify(value);

            return layoutTest;
        }
        private static VisualElement CreateSliderField(in string title, in float value, in float min, in float max, Action<float> onValueChangedCallback)
        {
            var layoutTest = new Box();
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("HorizontalLayout"));
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("Sliders"));
            layoutTest.AddToClassList("flex-horizontal");

            
            var floatSlider = new Slider(title, min, max);
            
            var floatField = new FloatField("");
            layoutTest.contentContainer.Add(floatSlider);
            layoutTest.contentContainer.Add(floatField);
            
            floatSlider.RegisterValueChangedCallback(evt =>
            {
                floatField.SetValueWithoutNotify(evt.newValue);
                onValueChangedCallback?.Invoke(evt.newValue);
                
            });
            
            var tempMin = min;
            var tempMax = max;
            floatField.RegisterValueChangedCallback(evt =>
            {
                var newValue = Mathf.Clamp(evt.newValue, tempMin, tempMax);
                
                floatSlider.SetValueWithoutNotify(newValue);
                
                onValueChangedCallback?.Invoke(newValue);
            });
            
            floatSlider.SetValueWithoutNotify(value);
            floatField.SetValueWithoutNotify(value);

            return layoutTest;
            //tempDialogueNode.mainContainer.Add(tierSli
        }
        
        private static VisualElement CreateInputField(in string title, in float value, in float min, in float max, Action<float> onValueChangedCallback)
        {
            var layoutTest = new Box();
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("HorizontalLayout"));
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("Inputs"));
            layoutTest.AddToClassList("flex-horizontal");

            var floatField = new FloatField(title);
            layoutTest.contentContainer.Add(floatField);

            var tempMin = min;
            var tempMax = max;
            floatField.RegisterValueChangedCallback(evt =>
            {
                var newValue = Mathf.Clamp(evt.newValue, tempMin, tempMax);
                
                floatField.SetValueWithoutNotify(newValue);
                onValueChangedCallback?.Invoke(newValue);
            });
            
            floatField.SetValueWithoutNotify(value);

            return layoutTest;
        }
        
        private static VisualElement CreateIntInputField(in string title, in int value, in int min, in int max, Action<int> onValueChangedCallback)
        {
            var layoutTest = new Box();
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("HorizontalLayout"));
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("Inputs"));
            layoutTest.AddToClassList("flex-horizontal");

            var integerField = new IntegerField(title);
            layoutTest.contentContainer.Add(integerField);

            var tempMin = min;
            var tempMax = max;
            integerField.RegisterValueChangedCallback(evt =>
            {
                var newValue = Mathf.Clamp(evt.newValue, tempMin, tempMax);
                
                integerField.SetValueWithoutNotify(newValue);
                onValueChangedCallback?.Invoke(newValue);
            });
            
            integerField.SetValueWithoutNotify(value);

            return layoutTest;
        }

        #endregion //Creating Visual Elements

        //====================================================================================================================//
        
    }
}
