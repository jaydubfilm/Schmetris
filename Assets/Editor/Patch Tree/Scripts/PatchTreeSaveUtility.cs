using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StarSalvager.Editor.PatchTrees.Graph;
using StarSalvager.Editor.PatchTrees.Nodes;
using StarSalvager.PatchTrees.Data;
using StarSalvager.ScriptableObjects.PatchTrees;
using StarSalvager.Utilities.Extensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

            var patchTreeContainer = GeneratePatchTreeContainer(graphView);

            if (patchTreeContainer == null)
                return;

            if (File.Exists(filePath))
            {
                var loadedFile = AssetDatabase.LoadAssetAtPath<PatchTreeContainer>(assetPath);

                loadedFile.NodeLinks = patchTreeContainer.NodeLinks;
                loadedFile.PartNodeData = patchTreeContainer.PartNodeData;
                loadedFile.PatchNodeDatas = new List<PatchNodeData>(patchTreeContainer.PatchNodeDatas);

                EditorUtility.SetDirty(loadedFile);
            }
            else
            {
                AssetDatabase.CreateAsset(patchTreeContainer, assetPath);
            }

            AssetDatabase.SaveAssets();
        }

        private static PatchTreeContainer GeneratePatchTreeContainer(in GraphView graphView)
        {
            var edges = graphView.edges.ToList();
            var nodes = graphView.nodes.ToList().Cast<BaseNode>().ToList();

            if (!edges.Any()) 
                return null;
            
            var patchTreeContainer = ScriptableObject.CreateInstance<PatchTreeContainer>();

            var connectedSockets = edges.Where(x => x.input.node != null).ToArray();
            foreach (var connectedSocket in connectedSockets)
            {
                var outputNode = (BaseNode)connectedSocket.output.node;
                var inputNode = (BaseNode)connectedSocket.input.node;

                patchTreeContainer.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID = outputNode.GUID,
                    PortName = $"{outputNode.title} -> {inputNode.title}",
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

            return patchTreeContainer;
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
            
            //ClearGraph(graphView, ref patchTreeContainer);
            GeneratePatchTreeNodes(graphView, patchTreeContainer);
            ConnectPatchTreeNodes(graphView, patchTreeContainer);
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
        private static void GeneratePatchTreeNodes(in PatchTreeGraphView graphView, in PatchTreeContainer patchTreeContainer)
        {
            //FIXME Find the existing Part Node
            var partNodeData = patchTreeContainer.PartNodeData;
            
            //Creating the Graphview generates the initial PartNode, so we need to apply the new data
            var partNode = (PartNode)graphView.nodes.ToList().FirstOrDefault(x => x is PartNode);
            partNode.LoadFromNodeData(partNodeData);

            foreach (var patchNodeData in patchTreeContainer.PatchNodeDatas)
            {
                var tempNode = (PatchNode)graphView.CreateNode(string.Empty, patchNodeData.Position, patchNodeData);
                
                graphView.AddElement(tempNode);
            }
        }

        private static void ConnectPatchTreeNodes(PatchTreeGraphView graphView,
            in PatchTreeContainer patchTreeContainer)
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

            foreach (var nodeLinkData in patchTreeContainer.NodeLinks)
            {
                var baseNode = nodes.FirstOrDefault(x => x.GUID == nodeLinkData.BaseNodeGUID);
                var targetNode = nodes.FirstOrDefault(x => x.GUID == nodeLinkData.TargetNodeGUID);

                var baseName = baseNode.title;
                var targetName = targetNode.title;
                
                if(baseNode.outputContainer.childCount == 0)
                    Debug.LogError($"{baseName} has no output");
                
                if(targetNode.inputContainer.childCount == 0)
                    Debug.LogError($"{targetName} has no Input Container");
                
                LinkNodesTogether((Port) baseNode.outputContainer[0], (Port) targetNode.inputContainer[0]);
            }

            /*for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var nodeGUID = node.GUID;

                var connections = patchTreeContainer.NodeLinks
                    .Where(x => x.BaseNodeGUID == nodeGUID)
                    .ToList();
                foreach (var targetNode in connections.Select(connection =>
                    nodes.First(x => x.GUID == connection.TargetNodeGUID)))
                {
                    LinkNodesTogether((Port) node.outputContainer[0], (Port) targetNode.inputContainer[0]);
                }
            }*/
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

        //Json I/O
        //====================================================================================================================//

        #region Json I/O

        public static string ExportJson(in GraphView graphView)
        {
            var patchTreeContainer = GeneratePatchTreeContainer(graphView);

            if (patchTreeContainer is null) throw new ArgumentException();

            return GetGraphAsJson(patchTreeContainer);
        }

        private static string GetGraphAsJson(in PatchTreeContainer patchTreeContainer)
        {
            var partNodeGUID = patchTreeContainer.PartNodeData.GUID;
            var orderedNodes = patchTreeContainer.PatchNodeDatas
                .OrderBy(x => x.Tier)
                .ThenByDescending(x => x.Position.y)
                .ToList();

            var outList = new List<PatchNodeJson>();
            
            foreach (var patchNodeData in orderedNodes)
            {
                var GUID = patchNodeData.GUID;

                var preReqs = patchTreeContainer.NodeLinks
                    .Where(x => !x.BaseNodeGUID.Equals(partNodeGUID))
                    .Where(x => x.TargetNodeGUID.Equals(GUID))
                    .Select(x => orderedNodes.FindIndex(y => y.GUID == x.BaseNodeGUID))
                    .ToArray();
                
                var newData = new PatchNodeJson
                {
                    Type = patchNodeData.Type,
                    Level = patchNodeData.Level,
                    Tier = patchNodeData.Tier,
                    PreReqs = preReqs
                };
                
                outList.Add(newData);
            }

            var json = JsonConvert.SerializeObject(outList);
            
            return json;
        }

        #endregion //Json I/O

        //====================================================================================================================//
        
        

    }
}
