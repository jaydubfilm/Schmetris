using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Editor.PatchTrees.Nodes;
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
            AddElement(CreatePatchNode(nodeName, new Vector2(100, 200)));
        }
        public void CreateNewPatchNode(string nodeName, Vector2 position)
        {
            AddElement(CreatePatchNode(nodeName, position));
        }

        public PatchNode CreatePatchNode(string nodeName, Vector2 position)
        {
            var tempDialogueNode = new PatchNode
            {
                title = PATCH_TYPE.EMPTY.ToString(),
                GUID = Guid.NewGuid().ToString(),
                PatchType = PATCH_TYPE.EMPTY
            };

            //--------------------------------------------------------------------------------------------------------//
            
            tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            
            var inputPort = GetPortInstance(tempDialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Pre Req";
            var outputPort = GetPortInstance(tempDialogueNode, Direction.Output, Port.Capacity.Multi);
            outputPort.portName = "Unlocks";
            tempDialogueNode.inputContainer.Add(inputPort);
            tempDialogueNode.inputContainer.Add(outputPort);
            
            tempDialogueNode.RefreshExpandedState();
            tempDialogueNode.RefreshPorts();
            tempDialogueNode.SetPosition(new Rect(position, DefaultNodeSize)); //To-Do: implement screen center instantiation positioning

            //--------------------------------------------------------------------------------------------------------//
            var enumField = new EnumField("Patch Type", PATCH_TYPE.EMPTY);
            enumField.styleSheets.Add(Resources.Load<StyleSheet>("EnumField"));
            tempDialogueNode.mainContainer.Add(enumField);
            
            tempDialogueNode.mainContainer.Add(CreateSliderField("Tier", 1 , 4));
            tempDialogueNode.mainContainer.Add(CreateSliderField("Level", 1 , 4));
            
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
            
            return tempDialogueNode;
        }
        public void AddChoicePort(Node nodeCache, string overriddenPortName = "")
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
        }

        private void RemovePort(Node node, Port socket)
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
        }

        private Port GetPortInstance(Node node, Direction nodeDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }
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

        private static VisualElement CreateSliderField(in string title, in int min, in int max)
        {
            var layoutTest = new Box();
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("HorizontalLayout"));
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("Sliders"));
            layoutTest.AddToClassList("flex-horizontal");

            
            var tierSlider = new SliderInt(title, min, max);
            
            var tierField = new IntegerField("");
            layoutTest.contentContainer.Add(tierSlider);
            layoutTest.contentContainer.Add(tierField);
            
            tierSlider.RegisterValueChangedCallback(evt =>
            {
                tierField.SetValueWithoutNotify(evt.newValue);
            });
            
            var tempMin = min;
            var tempMax = max;
            tierField.RegisterValueChangedCallback(evt =>
            {
                tierSlider.SetValueWithoutNotify(Mathf.Clamp(evt.newValue, tempMin, tempMax));
            });
            
            tierField.SetValueWithoutNotify(min);

            return layoutTest;
        }
        private static VisualElement CreateSliderField(in string title, in float min, in float max)
        {
            var layoutTest = new Box();
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("HorizontalLayout"));
            layoutTest.styleSheets.Add(Resources.Load<StyleSheet>("Sliders"));
            layoutTest.AddToClassList("flex-horizontal");

            
            var tierSlider = new Slider(title, min, max);
            
            var tierField = new FloatField("");
            layoutTest.contentContainer.Add(tierSlider);
            layoutTest.contentContainer.Add(tierField);
            
            tierSlider.RegisterValueChangedCallback(evt =>
            {
                tierField.SetValueWithoutNotify(evt.newValue);
            });
            
            var tempMin = min;
            var tempMax = max;
            tierField.RegisterValueChangedCallback(evt =>
            {
                tierSlider.SetValueWithoutNotify(Mathf.Clamp(evt.newValue, tempMin, tempMax));
            });
            
            tierField.SetValueWithoutNotify(min);

            return layoutTest;
            //tempDialogueNode.mainContainer.Add(tierSli
        }
    }
}
