using Spine.Unity;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager.AI
{
    [RequireComponent(typeof(MeshRenderer), typeof(SkeletonAnimation))]
    public abstract class SpineEnemy : Enemy, ISpineOverride
    {
        protected readonly struct SpineAnimation
        {
            public readonly string Name;
            public readonly bool Loops;

            public SpineAnimation(in string name, in bool loops)
            {
                Name = name;
                Loops = loops;
            }
        }

        public new MeshRenderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponent<MeshRenderer>();
                return _renderer;
            }
        }

        private MeshRenderer _renderer;

        public new SkeletonAnimation StateAnimator
        {
            get
            {
                if (_skeletonAnimation == null)
                    _skeletonAnimation = GetComponent<SkeletonAnimation>();

                return _skeletonAnimation;
            }
        }

        private SkeletonAnimation _skeletonAnimation;
        
        public override void SetSprite(Sprite sprite)
        {
            Debug.Log($"{nameof(SpineEnemy)} does not use {nameof(SpriteRenderer)}");
        }

        public override void SetColor(Color color)
        {
            Debug.Log($"{nameof(SpineEnemy)} does not use {nameof(SpriteRenderer)}");
        }
        
        public override void SetSortingLayer(string sortingLayerName, int sortingOrder = 0)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }

        protected override void SetAnimationController(in AnimationControllerScriptableObject animationController)
        {
            Debug.Log($"{nameof(SpineEnemy)} does not use {nameof(StateAnimator)}");
        }

        protected void SetSpineAnimation(in SpineAnimation spineAnimation)
        {
            StateAnimator.loop = spineAnimation.Loops;
            StateAnimator.AnimationName = spineAnimation.Name;
        }
    }
}
