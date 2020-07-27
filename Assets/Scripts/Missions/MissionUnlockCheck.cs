using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public abstract class MissionUnlockCheck
    {
        public bool IsComplete = false;
        public abstract bool CheckUnlockParameters();
    }
}