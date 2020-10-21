using System;
using UnityEngine;

namespace StarSalvager
{
    public interface ICanCombo<out TE> : ICanCombo where TE: Enum
    {
        TE Type { get; }
    }
    
    public interface ICanCombo
    {
        IAttachable iAttachable { get; }
        GameObject gameObject { get;}
        Transform transform { get; }
        Vector2Int Coordinate { get; }

        bool IsBusy { get; set; }

        int level { get; }
        
        void IncreaseLevel(int amount);
    }
}