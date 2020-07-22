using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class MissionAutoUnlockCheck : MissionUnlockCheck
    {
        public MissionAutoUnlockCheck() : base()
        {

        }
        
        public override bool CheckUnlockParameters()
        {
            return true;
        }
    }
}