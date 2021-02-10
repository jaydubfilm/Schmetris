using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public enum STATE
    {
        NONE,
        IDLE,
        SEARCH,
        MOVE,
        PURSUE,
        FLEE,
        ANTICIPATION,
        ATTACK,
        DEATH
    }
}
