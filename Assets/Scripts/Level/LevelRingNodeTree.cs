using StarSalvager.Factories;
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

    public LevelRingNode TryFindNode(int index)
    {
        return startingPoint.TryFindNode(index);
    }

    private bool TryAddNodeToTree(LevelRingNode newNode, int index)
    {
        return startingPoint.TryAddNode(newNode, index);
    }

    public int ConvertSectorWaveToNodeIndex(int sector, int wave)
    {
        int curIndex = 1;
        for (int i = 0; i < sector; i++)
        {
            curIndex += FactoryManager.Instance.SectorRemoteData[i].GetNumberOfWaves();
        }
        curIndex += wave;

        return curIndex;
    }

    public (int, int) ConvertNodeIndexIntoSectorWave(int nodeIndex)
    {
        if (nodeIndex == 0)
        {
            return (-1, -1);
        }
        
        int curSector = 0;
        int curWave = 0;

        for (int i = 1; i < nodeIndex; i++)
        {
            if (curWave + 1 < FactoryManager.Instance.SectorRemoteData[curSector].GetNumberOfWaves())
            {
                curWave++;
            }
            else
            {
                curSector++;
                curWave = 0;
            }
        }

        return (curSector, curWave);
    }
}
