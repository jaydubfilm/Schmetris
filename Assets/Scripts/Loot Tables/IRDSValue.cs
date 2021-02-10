using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IRDSValue<T> : IRDSObject
    {
        T rdsValue { get; }
    }
}