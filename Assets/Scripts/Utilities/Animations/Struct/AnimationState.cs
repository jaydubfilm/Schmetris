using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    [Serializable]
    public struct AnimationState
    {
        public int HashedID
        {
            get
            {
                if (_HashedID == 0)
                    _HashedID = Animator.StringToHash(StateName);
                
                return _HashedID;
            }
        }
        [NonSerialized, ReadOnly, ShowInInspector]
        public int _HashedID;
        
        [HorizontalGroup("State"), LabelWidth(75), DisableInPlayMode]
        public string StateName;
        
        [HorizontalGroup("State"), LabelWidth(75), DisableInPlayMode]
        public AnimationScriptableObject Animation;
    }
}

