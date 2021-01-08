using System;
using Recycling;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager
{
    [RequireComponent(typeof(SimpleAnimator))]
    public class AnimatedComponent :  Component, ICustomRecycle, ISimpleAnimation
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
        
        //============================================================================================================//

        public void CustomRecycle(params object[] args)
        {
            _simpleAnimator.Stop();
        }

        public Type GetOverrideType()
        {
            return typeof(AnimatedComponent);
        }

        //============================================================================================================//

    } 
}

