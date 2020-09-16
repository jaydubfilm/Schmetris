using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IPart: ILevel
    {
        bool Destroyed { get;}
        PART_TYPE Type { get; set; }
    }
}

