using Recycling;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    [RequireComponent(typeof(SimpleAnimator))]
    public class Explosion : MonoBehaviour, IRecycled
    {
        public bool IsRecycled { get; set; }
        
        private SimpleAnimator Animator => _animator ? _animator : _animator = GetComponent<SimpleAnimator>();
        private SimpleAnimator _animator;

        public new Transform transform => _transform ? _transform : _transform = gameObject.transform;
        private Transform _transform;
        
        //============================================================================================================//

        private void OnEnable()
        {
            Animator.Play();
        }

        private void LateUpdate()
        {
            if (IsRecycled)
                return;

            //When the animation is done playing, we recycle it
            if (Animator.Playing)
                return;
            
            Recycler.Recycle<Explosion>(this);
        }

        
        //============================================================================================================//
        
    }
}


