using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    public class StateAnimator : SimpleAnimator
    {
        [SerializeField]
        private AnimationControllerScriptableObject animationController;

        private new AnimationScriptableObject animation;

        protected override void Start()
        {
            base.Start();

            SetAnimation(animationController.GetDefaultAnimation());
        }

        public void ChangeState(string newStateName)
        {
            SetAnimation(animationController.GetAnimation(newStateName));
        }
    }
}


