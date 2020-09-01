using System;
using UnityEngine;

namespace StarSalvager
{
    public interface ICanCombo<out T> : ICanCombo where T: Enum
    {
        T Type { get; }
    }
    
    public interface ICanCombo
    {
        IAttachable iAttachable { get; }
        GameObject gameObject { get;}
        Transform transform { get; }

        Vector2Int Coordinate { get; }

        int level { get; }
        
        void IncreaseLevel(int amount);
    }
}