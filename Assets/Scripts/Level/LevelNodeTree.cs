using StarSalvager.Factories;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNodeTree
{
    private LevelNode startingPoint;

    public LevelNodeTree()
    {
        startingPoint = new LevelNode(0);
    }

    public void ReadInNodeConnectionData(List<Vector2Int> nodeConnections, List<int> wreckNodes)
    {
        for (int i = 0; i < nodeConnections.Count; i++)
        {
            LevelNode newNode = TryFindNode(nodeConnections[i].x);
            if (newNode == null)
            {
                newNode = new LevelNode(nodeConnections[i].x);
            }

            bool successfullyParent = TryAddNodeToTree(newNode, nodeConnections[i].y);

            if (!successfullyParent)
            {
                Debug.LogError("Reading in Node Connection Data for LevelRingNodeTree: Failed to find node " + newNode.nodeIndex + " 'parent with index " + nodeConnections[i].y);
            }
        }

        for (int i = 0; i < wreckNodes.Count; i++)
        {
            TryFindNode(wreckNodes[i]).isWreckNode = true;
        }
    }

    public List<Vector2Int> ConvertNodeTreeIntoConnections()
    {
        return startingPoint.ConvertNodeTreeIntoConnections();
    }

    public LevelNode TryFindNode(int index)
    {
        return startingPoint.TryFindNode(index);
    }

    private bool TryAddNodeToTree(LevelNode newNode, int index)
    {
        return startingPoint.TryAddNode(newNode, index);
    }

    public int ConvertSectorWaveToNodeIndex(int sector, int wave)
    {
        int curIndex = 1;
        for (int i = 0; i < sector; i++)
        {
            curIndex += 5;
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
