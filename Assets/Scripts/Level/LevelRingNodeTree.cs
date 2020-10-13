using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRingNodeTree
{
    private LevelRingNode startingPoint;

    public LevelRingNodeTree()
    {
        startingPoint = new LevelRingNode(0);
    }

    public void ReadInNodeConnectionData(List<Vector2Int> nodeConnections)
    {
        for (int i = 0; i < nodeConnections.Count; i++)
        {
            LevelRingNode newNode = TryFindNode(nodeConnections[i].x);
            if (newNode == null)
            {
                newNode = new LevelRingNode(nodeConnections[i].x);
            }

            bool successfullyParent = TryAddNodeToTree(newNode, nodeConnections[i].y);

            if (!successfullyParent)
            {
                Debug.LogError("Reading in Node Connection Data for LevelRingNodeTree: Failed to find node " + newNode.nodeIndex + " 'parent with index " + nodeConnections[i].y);
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
