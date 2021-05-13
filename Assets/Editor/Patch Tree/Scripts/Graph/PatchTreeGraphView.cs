using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Editor.PatchTrees.Nodes;
using StarSalvager.PatchTrees;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StarSalvager.Editor.PatchTrees.Graph
{
    //Based on: https://github.com/merpheus-dev/NodeBasedDialogueSystem/blob/master/com.subtegral.dialoguesystem/Editor/Graph/StoryGraphView.cs
    public class PatchTreeGraphView : GraphView
    {
        public readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public readonly Vector2 DefaultCommentBlockSize = new Vector2(300, 200);
        
        public PatchTreeGraphView(PatchTreeWindow editorWindow)
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
            
            AddElement(GetEntryPointNodeInstance());
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var startPortView = startPort;

            ports.ForEach((port) =>
            {
                var portView = port;
                if (startPortView != portView && startPortView.node != portView.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void CreateNewPatchNode(string nodeName)
        {
            AddElement(CreateNode(nodeName, new Vector2(100, 200), 
                new PatchNodeData
                {
                    GUID = Guid.NewGuid().ToString(),
                    Type = (int)PATCH_TYPE.EMPTY,
                    Tier = 1
                }));
        }
        public void CreateNewPatchNode(string nodeName, Vector2 position)
        {
            AddElement(CreateNode(nodeName, position, 
                new PatchNodeData
                {
                    GUID = Guid.NewGuid().ToString(),
                    Type = (int)PATCH_TYPE.EMPTY,
                    Tier = 1
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
            });
            tempNode.mainContainer.Add(enumField);

            
            tempNode.mainContainer.Add(CreateSliderField("Tier",patchNode.Tier, 1 , 4, tier =>
            {
                patchNode.Tier = tier;
            }));
            tempNode.mainContainer.Add(CreateSliderField("Level",patchNode.Level, 1 , 4, level =>
            {
                patchNode.Level = level;
            }));
            
            //--------------------------------------------------------------------------------------------------------//
            
            /*var textField = new TextField("");
            textField.RegisterValueChangedCallback(evt =>
            {
                //tempDialogueNode.PatchType = evt.newValue;
                tempDialogueNode.title = evt.newValue;
            });*/
            /*textField.SetValueWithoutNotify(tempDialogueNode.title);
            tempDialogueNode.mainContainer.Add(textField);*/

            //--------------------------------------------------------------------------------------------------------//
            
            /*var button = new Button(() => { AddChoicePort(tempDialogueNode); })
            {
                text = "Add Choice"
            };
            tempDialogueNode.titleButtonContainer.Add(button);*/

            //--------------------------------------------------------------------------------------------------------//
            
            return tempNode;
        }
        /*public void AddChoicePort(Node nodeCache, string overriddenPortName = "")
        {
            var generatedPort = GetPortInstance(nodeCache, Direction.Output);
            var portLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(portLabel);

            var outputPortCount = nodeCache.outputContainer.Query("connector").ToList().Count();
            var outputPortName = string.IsNullOrEmpty(overriddenPortName)
                ? $"Option {outputPortCount + 1}"
                : overriddenPortName;


            var textField = new TextField()
            {
                name = string.Empty,
                value = outputPortName
            };
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(nodeCache, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = outputPortName;
            nodeCache.outputContainer.Add(generatedPort);
            nodeCache.RefreshPorts();
            nodeCache.RefreshExpandedState();
        }*/

        /*private void RemovePort(Node node, Port socket)
        {
            var targetEdge = edges.ToList()
                .Where(x => x.output.portName == socket.portName && x.output.node == socket.node)
                .ToList();
            
            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            node.outputContainer.Remove(socket);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }*/

        private Port GetPortInstance(Node node, Direction nodeDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }
        
        //TODO Need to finish integrating this
        private PartNode GetEntryPointNodeInstance()
        {
            var nodeCache = new PartNode
            {
                //title = "START",
                GUID = Guid.NewGuid().ToString(),
                PartType = PART_TYPE.CORE
                //DialogueText = "ENTRYPOINT",
                //EntyPoint = true
            };
            nodeCache.Updated();

            var generatedPort = GetPortInstance(nodeCache, Direction.Output, Port.Capacity.Multi);
            generatedPort.portName = "Patches";
            nodeCache.outputContainer.Add(generatedPort);

            nodeCache.capabilities &= ~Capabilities.Movable;
            nodeCache.capabilities &= ~Capabilities.Deletable;

            nodeCache.RefreshExpandedState();
            nodeCache.RefreshPorts();
            nodeCache.SetPosition(new Rect(100, 200, 100, 150));
            return nodeCache;
        }

        private static VisualElement CreateEnumField<T>(in string title, in T value, in T @default, Action<T> onValueChanged) where T : Enum
        {
            var enumField = new EnumField(title, default);
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
    }
}
