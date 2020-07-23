using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    public class SimpleAnimator : MonoBehaviour
    {
        [SerializeField, Required]
        private SpriteRenderer targetRenderer;

        [SerializeField, Range(0.1f, 2f)]
        public float speed = 1f;
        [SerializeField]
        private bool playOnAwake = true;

        [SerializeField]
        private AnimationScriptableObject animation;

        private float _t;

        public float Alpha
        {
            set
            {
                var color = targetRenderer.color;
                color.a = value;
                targetRenderer.color = color;
            }
        }

        private bool _playing;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        protected virtual void Start()
        {
            if(targetRenderer is null)
                throw new Exception($"No {nameof(targetRenderer)} set on {gameObject.name}");

            if (playOnAwake)
                _playing = true;

            
            if (animation == null)
                _playing = false;
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if (!_playing)
                return;

            targetRenderer.sprite = animation.PlayFrame(speed, ref _t);
        }
        
        //============================================================================================================//

        public void Play()
        {
            if (animation == null)
                return;
            
            _playing = true;
        }

        public void Stop()
        {
            _playing = false;
            _t = 0f;
            targetRenderer.sprite = null;
        }

        public void SetAnimation(AnimationScriptableObject animation)
        {
            this.animation = animation;
            
            if(playOnAwake && this.animation != null)
                _playing = true;
        }

        //============================================================================================================//
    }
}

