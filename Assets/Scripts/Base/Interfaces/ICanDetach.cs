using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface ICanDetach
    {
        int AttachPriority { get; }
    }
}
