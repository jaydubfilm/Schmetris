using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRingNodeTree
{
    private LevelRingNode startingPoint;

    public LevelRingNodeTree()
    {
        startingPoint = new LevelRingNode(0);

        /*List<Vector2Int> nodeConnectionsList = new List<Vector2Int>();

        nodeConnectionsList.Add(new Vector2Int(1, 0));
        nodeConnectionsList.Add(new Vector2Int(2, 1));
        nodeConnectionsList.Add(new Vector2Int(2, 0));
        nodeConnectionsList.Add(new Vector2Int(4, 2));
        //nodeConnectionsList.Add(new Vector2Int(4, 3));

        ReadInNodeConnectionData(nodeConnectionsList);

        List<Vector2Int> output = ConvertNodeTreeIntoConnections();
        for (int i = 0; i < output.Count; i++)
        {
            Debug.Log(output[i]);
        }*/
    }

    public void ReadInNodeConnectionData(List<Vector2Int> nodeConnections)
    {
        for (int i = 0; i < nodeConnections.Count; i++)
        {
            LevelRingNode newNode = TryFindNode(nodeConnections[i].x);
            if (newNode == null)
            {
                newNode = new LevelRingNode(nodeConnections[i].x);
                Debug.Log("Create new node " + newNode.nodeIndex);
            }

            bool successfullyParent = TryAddNodeToTree(newNode, nodeConnections[i].y);

            if (!successfullyParent)
            {
                Debug.LogError("Reading in Node Connection Data for LevelRingNodeTree: Failed to find node " + newNode.nodeIndex + " 'parent with index " + nodeConnections[i].y);
            }
            else
            {
                Debug.Log("Parent " + newNode.nodeIndex + " to " + nodeConnections[i].y);
            }
        }
    }

    public List<Vector2Int> ConvertNodeTreeIntoConnections()
    {
        return startingPoint.ConvertNodeTreeIntoConnections();
    }

    private LevelRingNode TryFindNode(int index)
    {
        return startingPoint.TryFindNode(index);
    }

    private bool TryAddNodeToTree(LevelRingNode newNode, int index)
    {
        return startingPoint.TryAddNode(newNode, index);
    }
}
