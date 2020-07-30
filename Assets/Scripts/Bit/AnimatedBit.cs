using System;
using Recycling;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager
{
    [RequireComponent(typeof(SimpleAnimator))]
    public class AnimatedBit : Bit, IOverrideRecycleType, ISimpleAnimation
    {
        public SimpleAnimator SimpleAnimator
        {
            get
            {
                if (!_simpleAnimator)
                    _simpleAnimator = GetComponent<SimpleAnimator>();
                
                return _simpleAnimator;
            }
        }
        private SimpleAnimator _simpleAnimator;

        //============================================================================================================//
        
        public override void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);
            
            _simpleAnimator.Stop();
        }

        public Type GetOverrideType()
        {
            return typeof(AnimatedBit);
        }

        //============================================================================================================//

    }
}

