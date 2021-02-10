using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IRDSTable : IRDSObject
    {
        Vector2Int rdsCount { get; set; }       // How many items shall drop from this table?
        IEnumerable<IRDSObject> rdsContents { get; } // The contents of the table
        IEnumerable<IRDSObject> rdsResult { get; }   // The Result set
    }
}   