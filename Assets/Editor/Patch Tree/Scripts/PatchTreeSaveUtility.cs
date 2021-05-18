using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StarSalvager.Editor.PatchTrees.Graph;
using StarSalvager.Editor.PatchTrees.Nodes;
using StarSalvager.PatchTrees;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace StarSalvager.Editor.PatchTrees
{
    public class PatchTreeSaveUtility
    {
        private List<Edge> Edges => _graphView.edges.ToList();
        private List<BaseNode> Nodes => _graphView.nodes.ToList().Cast<BaseNode>().ToList();

        /*private List<Group> CommentBlocks =>
            _graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();*/

        private PatchTreeContainer _patchTreeContainer;
        private PatchTreeGraphView _graphView;

        public static PatchTreeSaveUtility GetInstance(PatchTreeGraphView graphView)
        {
            return new PatchTreeSaveUtility
            {
                _graphView = graphView
            };
        }

        public void SaveGraph(string fileName)
        {

            //--------------------------------------------------------------------------------------------------------//
            
            const string DIRECTORY = "/Scriptable Objects/Patch Trees/";

            string GetAssetPath(in string filename) => $"Assets{DIRECTORY}{filename}.asset";
            string GetFilePath(in string filename) => $"{Application.dataPath}{DIRECTORY}{filename}.asset";

            //--------------------------------------------------------------------------------------------------------//

            var assetPath = GetAssetPath(fileName);
            var filePath = GetFilePath(fileName);
            
            var dialogueContainerObject = ScriptableObject.CreateInstance<PatchTreeContainer>();
            if (!SaveNodes(fileName, ref dialogueContainerObject)) 
                return;

            if (File.Exists(filePath))
            {
                var loadedFile = AssetDatabase.LoadAssetAtPath<PatchTreeContainer>(assetPath);
                
                loadedFile.NodeLinks = dialogueContainerObject.NodeLinks;
                loadedFile.PartNodeData = dialogueContainerObject.PartNodeData;
                loadedFile.PatchNodeDatas = new List<PatchNodeData>(dialogueContainerObject.PatchNodeDatas);

                EditorUtility.SetDirty(loadedFile);
            }
            else
            {
                AssetDatabase.CreateAsset(dialogueContainerObject, assetPath);
            }

            AssetDatabase.SaveAssets();
        }

        private bool SaveNodes(string fileName, ref PatchTreeContainer dialogueContainerObject)
        {
            if (!Edges.Any()) return false;
            
            var connectedSockets = Edges.Where(x => x.input.node != null).ToArray();
            for (var i = 0; i < connectedSockets.Length; i++)
            {
                var outputNode = (connectedSockets[i].output.node as BaseNode);
                var inputNode = (connectedSockets[i].input.node as BaseNode);
                
                dialogueContainerObject.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID = outputNode.GUID,
                    PortName = connectedSockets[i].output.portName,
                    TargetNodeGUID = inputNode.GUID
                });
            }

            foreach (var node in Nodes)
            {
                switch (node)
                {
                    case PartNode partNode:
                        dialogueContainerObject.PartNodeData = partNode.GetNodeData();
                        continue;
                    case PatchNode patchNode:
                        var nodeData = patchNode.GetNodeData();
                        dialogueContainerObject.PatchNodeDatas.Add(nodeData);
                        break;
                }
            }

            return true;
        }

        /*private void SaveExposedProperties(PatchTreeContainer dialogueContainer)
        {
            dialogueContainer.ExposedProperties.Clear();
            dialogueContainer.ExposedProperties.AddRange(_graphView.ExposedProperties);
        }*/

        /*private void SaveCommentBlocks(DialogueContainer dialogueContainer)
        {
            foreach (var block in CommentBlocks)
            {
                var nodes = block.containedElements.Where(x => x is DialogueNode).Cast<DialogueNode>().Select(x => x.GUID)
                    .ToList();

                dialogueContainer.CommentBlockData.Add(new CommentBlockData
                {
                    ChildNodes = nodes,
                    Title = block.title,
                    Position = block.GetPosition().position
                });
            }
        }*/

        public void LoadPatchTree(string fileName)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            const string DIRECTORY = "/Scriptable Objects/Patch Trees/";

            string GetAssetPath(in string filename) => $"Assets{DIRECTORY}{filename}.asset";
            string GetFilePath(in string filename) => $"{Application.dataPath}{DIRECTORY}{filename}.asset";

            //--------------------------------------------------------------------------------------------------------//
            
            var assetPath = GetAssetPath(fileName);
            var filePath = GetFilePath(fileName);
            
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("File Not Found", "Target Narrative Data does not exist!", "OK");
                return;
            }
            
            _patchTreeContainer = AssetDatabase.LoadAssetAtPath<PatchTreeContainer>(assetPath);
            
            ClearGraph();
            GeneratePatchTreeNodes();
            ConnectPatchTreeNodes();
            //AddExposedProperties();
            //GenerateCommentBlocks();
        }

        /// <summary>
        /// Set Entry point GUID then Get All Nodes, remove all and their edges. Leave only the entrypoint node. (Remove its edge too)
        /// </summary>
        private void ClearGraph()
        {
            Nodes.Find(x => x is PartNode).GUID = _patchTreeContainer.NodeLinks[0].BaseNodeGUID;
            foreach (var perNode in Nodes)
            {
                if (perNode is PartNode) 
                    continue;
                
                Edges.Where(x => x.input.node == perNode).ToList()
                    .ForEach(edge => _graphView.RemoveElement(edge));
                _graphView.RemoveElement(perNode);
            }
        }

        /// <summary>
        /// Create All serialized nodes and assign their guid and dialogue text to them
        /// </summary>
        private void GeneratePatchTreeNodes()
        {
            //FIXME Find the existing Part Node
            var partNode = _patchTreeContainer.PartNodeData;
            _graphView.CreateNode(string.Empty, new Vector2(partNode.Position.x, partNode.Position.y), _patchTreeContainer.PartNodeData);
            
            foreach (var patchNodeData in _patchTreeContainer.PatchNodeDatas)
            {
                var tempNode = (PatchNode)_graphView.CreateNode(string.Empty, patchNodeData.Position, patchNodeData);
                
                _graphView.AddElement(tempNode);
            }
        }

        private void ConnectPatchTreeNodes()
        {

            //--------------------------------------------------------------------------------------------------------//
            
            void LinkNodesTogether(Port outputSocket, Port inputSocket)
            {
                var tempEdge = new Edge
                {
                    output = outputSocket,
                    input = inputSocket
                };
                tempEdge.input.Connect(tempEdge);
                tempEdge.output.Connect(tempEdge);
                _graphView.Add(tempEdge);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            for (var i = 0; i < Nodes.Count; i++)
            {
                var k = i; //Prevent access to modified closure
                var connections = _patchTreeContainer.NodeLinks
                    .Where(x => x.BaseNodeGUID == Nodes[k].GUID)
                    .ToList();
                var node = Nodes[i];
                foreach (var targetNode in connections.Select(connection => Nodes.First(x => x.GUID == connection.TargetNodeGUID)))
                {
                    LinkNodesTogether((Port) node.outputContainer[0], (Port) targetNode.inputContainer[0]);
                }
            }
        }

        /*private void AddExposedProperties()
        {
            _graphView.ClearBlackBoardAndExposedProperties();
            foreach (var exposedProperty in _dialogueContainer.ExposedProperties)
            {
                _graphView.AddPropertyToBlackBoard(exposedProperty);
            }
        }*/

        /*private void GenerateCommentBlocks()
        {
            foreach (var commentBlock in CommentBlocks)
            {
                _graphView.RemoveElement(commentBlock);
            }

            foreach (var commentBlockData in _dialogueContainer.CommentBlockData)
            {
               var block = _graphView.CreateCommentBlock(new Rect(commentBlockData.Position, _graphView.DefaultCommentBlockSize),
                    commentBlockData);
               block.AddElements(Nodes.Where(x=>commentBlockData.ChildNodes.Contains(x.GUID)));
            }
        }*/
    }
}
