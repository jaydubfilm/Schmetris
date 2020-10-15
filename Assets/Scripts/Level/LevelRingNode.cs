using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRingNode : IEquatable<LevelRingNode>
{
    public int nodeIndex;

    public List<LevelRingNode> childNodes = new List<LevelRingNode>();

    public LevelRingNode(int index)
    {
        nodeIndex = index;
    }

    public List<Vector2Int> ConvertNodeTreeIntoConnections()
    {
        List<Vector2Int> nodeConnectionsList = new List<Vector2Int>();

        for (int i = 0; i < childNodes.Count; i++)
        {
            nodeConnectionsList.Add(new Vector2Int(childNodes[i].nodeIndex, nodeIndex));
            nodeConnectionsList.AddRange(childNodes[i].ConvertNodeTreeIntoConnections());
        }

        return nodeConnectionsList;
    }

    public LevelRingNode TryFindNode(int index)
    {
        if (Equals(index))
        {
            return this;
        }

        for (int i = 0; i < childNodes.Count; i++)
        {
            LevelRingNode node = childNodes[i].TryFindNode(index);
            if (node != null)
            {
                return node;
            }
        }

        return null;
    }

    public bool TryAddNode(LevelRingNode newNode, int index)
    {
        if (Equals(index))
        {
            childNodes.Add(newNode);
            return true;
        }

        for (int i = 0; i < childNodes.Count; i++)
        {
            bool addedToChild = childNodes[i].TryAddNode(newNode, index);
            if (addedToChild)
            {
                return true;
            }
        }

        return false;
    }

    #region IEquatable

    /// <summary>
    /// This only compares Type and not all individual properties
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(LevelRingNode other)
    {
        return nodeIndex == other.nodeIndex;
    }

    public bool Equals(int index)
    {
        return nodeIndex == index;
    }

    /// <summary>
    /// This only compares Type and not all individual properties
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        return obj is LevelRingNode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion //IEquatable
}
