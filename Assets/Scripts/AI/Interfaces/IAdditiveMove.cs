using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IAdditiveMove
    {
        Transform transform { get; }
        GameObject gameObject { get; }

        Vector2 AddMove { get; }
    }
}
