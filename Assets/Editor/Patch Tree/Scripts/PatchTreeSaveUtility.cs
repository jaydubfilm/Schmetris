using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StarSalvager.Editor.PatchTrees.Graph;
using StarSalvager.Editor.PatchTrees.Nodes;
using StarSalvager.PatchTrees;
using StarSalvager.Utilities.Extensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace StarSalvager.Editor.PatchTrees
{
    public static class PatchTreeSaveUtility
    {
        const string DIRECTORY = "/Scriptable Objects/Patch Trees/";

        //Saving Graph
        //====================================================================================================================//
        
        public static void SaveGraph(in PART_TYPE partType, in PatchTreeGraphView graphView)
        {
            var fileName = GetFileName(partType);
            
            //--------------------------------------------------------------------------------------------------------//

            const string DIRECTORY = "/Scriptable Objects/Patch Trees/";

            string GetAssetPath(in string filename) => $"Assets{DIRECTORY}{filename}.asset";
            string GetFilePath(in string filename) => $"{Application.dataPath}{DIRECTORY}{filename}.asset";

            //--------------------------------------------------------------------------------------------------------//

            var assetPath = GetAssetPath(fileName);
            var filePath = GetFilePath(fileName);

            var dialogueContainerObject = ScriptableObject.CreateInstance<PatchTreeContainer>();
            if (!SaveNodes(graphView, ref dialogueContainerObject))
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

        private static bool SaveNodes(PatchTreeGraphView graphView,
            ref PatchTreeContainer patchTreeContainer)
        {
            var edges = graphView.edges.ToList();
            var nodes = graphView.nodes.ToList().Cast<BaseNode>().ToList();

            if (!edges.Any()) return false;

            var connectedSockets = edges.Where(x => x.input.node != null).ToArray();
            for (var i = 0; i < connectedSockets.Length; i++)
            {
                var outputNode = (connectedSockets[i].output.node as BaseNode);
                var inputNode = (connectedSockets[i].input.node as BaseNode);

                patchTreeContainer.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID = outputNode.GUID,
                    PortName = connectedSockets[i].output.portName,
                    TargetNodeGUID = inputNode.GUID
                });
            }

            foreach (var node in nodes)
            {
                switch (node)
                {
                    case PartNode partNode:
                        patchTreeContainer.PartNodeData = partNode.GetNodeData();
                        continue;
                    case PatchNode patchNode:
                        var nodeData = patchNode.GetNodeData();
                        patchTreeContainer.PatchNodeDatas.Add(nodeData);
                        break;
                }
            }

            return true;
        }

        //Loading Graph
        //====================================================================================================================//

        public static void LoadPatchTree(in PART_TYPE partType, in PatchTreeGraphView graphView)
        {
            var fileName = GetFileName(partType);
            
            //--------------------------------------------------------------------------------------------------------//

            string GetAssetPath(in string filename) => $"Assets{DIRECTORY}{filename}.asset";

            //--------------------------------------------------------------------------------------------------------//
            
            var assetPath = GetAssetPath(fileName);
            
            if (!DoesPatchTreeExist(fileName))
            {
                EditorUtility.DisplayDialog("File Not Found", "Target Narrative Data does not exist!", "OK");
                return;
            }
            
            var patchTreeContainer = AssetDatabase.LoadAssetAtPath<PatchTreeContainer>(assetPath);
            
            ClearGraph(graphView, ref patchTreeContainer);
            GeneratePatchTreeNodes(graphView, ref patchTreeContainer);
            ConnectPatchTreeNodes(graphView, ref patchTreeContainer);
            //AddExposedProperties();
            //GenerateCommentBlocks();
        }

        //====================================================================================================================//
        
        /// <summary>
        /// Set Entry point GUID then Get All Nodes, remove all and their edges. Leave only the entrypoint node. (Remove its edge too)
        /// </summary>
        private static void ClearGraph(PatchTreeGraphView graphView, ref PatchTreeContainer patchTreeContainer)
        {
            var edges = graphView.edges.ToList();
            var nodes = graphView.nodes.ToList().Cast<BaseNode>().ToList();

            if (nodes.IsNullOrEmpty() || patchTreeContainer.NodeLinks.IsNullOrEmpty())
                return;
            
            nodes.Find(x => x is PartNode).GUID = patchTreeContainer.NodeLinks[0].BaseNodeGUID;
            foreach (var perNode in nodes)
            {
                if (perNode is PartNode) 
                    continue;
                
                edges.Where(x => x.input.node == perNode).ToList().ForEach(graphView.RemoveElement);
                
                graphView.RemoveElement(perNode);
            }
        }

        /// <summary>
        /// Create All serialized nodes and assign their guid and dialogue text to them
        /// </summary>
        private static void GeneratePatchTreeNodes(in PatchTreeGraphView graphView, ref PatchTreeContainer patchTreeContainer)
        {
            //FIXME Find the existing Part Node
            var partNode = patchTreeContainer.PartNodeData;
            graphView.CreateNode(string.Empty, new Vector2(partNode.Position.x, partNode.Position.y), patchTreeContainer.PartNodeData);
            
            foreach (var patchNodeData in patchTreeContainer.PatchNodeDatas)
            {
                var tempNode = (PatchNode)graphView.CreateNode(string.Empty, patchNodeData.Position, patchNodeData);
                
                graphView.AddElement(tempNode);
            }
        }

        private static void ConnectPatchTreeNodes(PatchTreeGraphView graphView, ref PatchTreeContainer patchTreeContainer)
        {
            var nodes = graphView.nodes.ToList().Cast<BaseNode>().ToList();
            
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
                graphView.Add(tempEdge);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            for (var i = 0; i < nodes.Count; i++)
            {
                var k = i; //Prevent access to modified closure
                var connections = patchTreeContainer.NodeLinks
                    .Where(x => x.BaseNodeGUID == nodes[k].GUID)
                    .ToList();
                var node = nodes[i];
                foreach (var targetNode in connections.Select(connection => nodes.First(x => x.GUID == connection.TargetNodeGUID)))
                {
                    LinkNodesTogether((Port) node.outputContainer[0], (Port) targetNode.inputContainer[0]);
                }
            }
        }

        //DoesPatchTreeExist
        //====================================================================================================================//
        
        public static bool DoesPatchTreeExist(in string fileName)
        {
            string GetFilePath(in string filename) => $"{Application.dataPath}{DIRECTORY}{filename}.asset";
            var filePath = GetFilePath(fileName);

            return File.Exists(filePath);
        }


        private static string GetFileName(in PART_TYPE partType) => $"{partType.ToString()}_PatchTree";

        //====================================================================================================================//

    }
}
