﻿using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    public class StateAnimator : SimpleAnimator
    {
        public const string DEFAULT = "Default";
        
        [SerializeField]
        private AnimationControllerScriptableObject animationController;

        public void ChangeState(string newStateName)
        {
            var animation = animationController.GetAnimation(newStateName);

            SetAnimation(animation);
        }
        
        public void ChangeState(int animationId)
        {
            var animation = animationController.GetAnimation(animationId);
            
            SetAnimation(animation);
        }
        
        public void SetController(AnimationControllerScriptableObject animationController)
        {
            if(animationController == null)
                return;
            
            this.animationController = animationController;
            SetAnimation(animationController.GetDefaultAnimation());
        }
    }
}


