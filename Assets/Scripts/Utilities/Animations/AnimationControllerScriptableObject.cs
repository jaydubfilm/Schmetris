using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    [CreateAssetMenu(fileName = "Animation Controller", menuName = "Star Salvager/Scriptable Objects/Animation Controller")]
    public class AnimationControllerScriptableObject : ScriptableObject
    {
        [SerializeField]
        private AnimationState[] States;

        public AnimationScriptableObject GetDefaultAnimation()
        {
            return States[0].Animation;
        }

        public AnimationScriptableObject GetAnimation(string StateName)
        {
            var hashed = Animator.StringToHash(StateName);

            return GetAnimation(hashed);
        }
        
        public AnimationScriptableObject GetAnimation(int hashID)
        {
            return States.FirstOrDefault(x => x.HashedID == hashID).Animation;
        }
    }

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
        
        [HorizontalGroup("State"), LabelWidth(75)]
        public string StateName;

        
        
        [HorizontalGroup("State"), LabelWidth(75)]
        public AnimationScriptableObject Animation;
    }
}

