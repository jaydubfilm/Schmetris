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

        public bool Playing => _playing;
        
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

            if (!animation.Loop && _t >= 1f)
            {
                Stop();
                return;
            }
            
            targetRenderer.sprite = animation.PlayFrame(speed, ref _t);

        }
        
        //============================================================================================================//

        [Button, HorizontalGroup("Row1"), DisableInEditorMode, DisableIf("_playing")]
        public void Play()
        {
            if (animation == null)
                return;
            
            _playing = true;
        }
        //[Button, HorizontalGroup("Row1"), DisableInEditorMode, EnableIf("_playing")]
        public void Pause()
        {
            _playing = false;
            //_t = 0f;
            //targetRenderer.sprite = null;
        }
        [Button, HorizontalGroup("Row1"), DisableInEditorMode, EnableIf("_playing")]
        public void Stop()
        {
            _playing = false;
            _t = 0f;
            targetRenderer.sprite = null;
        }

        public void SetAnimation(AnimationScriptableObject animation)
        {
            this.animation = animation;
            _t = 0f;

            if (playOnAwake && this.animation != null)
            {
                _playing = true;
                targetRenderer.sprite = animation.GetFrame(0);
            }
        }

        //============================================================================================================//
    }
}

