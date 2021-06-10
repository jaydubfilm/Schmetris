using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    public class StateAnimator : SimpleAnimator
    {
        public const string DEFAULT = "Default";
        
        [SerializeField]
        private AnimationControllerScriptableObject animationController;

        public void ChangeState(in string newStateName)
        {
            var animation = animationController.GetAnimation(newStateName);

            SetAnimation(animation);
        }
        
        public void ChangeState(in int animationId)
        {
            var animation = animationController.GetAnimation(animationId);
            
            SetAnimation(animation);
        }
        
        public void SetController(in AnimationControllerScriptableObject animationController)
        {
            if(animationController == null)
                return;
            
            this.animationController = animationController;
            SetAnimation(animationController.GetDefaultAnimation());
        }
    }
}


