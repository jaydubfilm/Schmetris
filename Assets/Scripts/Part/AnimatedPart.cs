using System;
using Recycling;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager
{
    [RequireComponent(typeof(SimpleAnimator))]
    public class AnimatedPart : Part, IOverrideRecycleType, ISimpleAnimation
    {
        public SimpleAnimator SimpleAnimator
        {
            get
            {
                if (_simpleAnimator == null)
                    _simpleAnimator = GetComponent<SimpleAnimator>();

                return _simpleAnimator;
            }
        }

        private SimpleAnimator _simpleAnimator;
    
        //================================================================================================================//
    
        public Type GetOverrideType()
        {
            return typeof(AnimatedPart);
        }
    }

}
