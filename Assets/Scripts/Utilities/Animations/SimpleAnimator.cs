using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    public class SimpleAnimator : MonoBehaviour
    {
        [SerializeField, Required]
        private SpriteRenderer targetRenderer;
        
        [SerializeField]
        private List<Sprite> sprites;

        [SerializeField, Range(1f, 10f)]
        public float speed;

        [SerializeField]
        private bool playOnAwake = true;

        [SerializeField, ToggleGroup("useAnimationCurve")]
        private bool useAnimationCurve;
        
        [SerializeField, ToggleGroup("useAnimationCurve")]
        private AnimationCurve curve = new AnimationCurve();

        [SerializeField, ReadOnly]
        private float t;

        public float Alpha
        {
            set
            {
                var color = targetRenderer.color;
                color.a = value;
                targetRenderer.color = color;
            }
        }

        private float _max;

        private bool _playing;

        private int _spriteCount;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            if(targetRenderer is null)
                throw new Exception($"No {nameof(targetRenderer)} set on {gameObject.name}");

            if (playOnAwake)
                _playing = true;

            _spriteCount = sprites.Count - 1;
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_playing)
                return;

            var value = 1f / speed;

            t = Mathf.PingPong(Time.time, value) / value;
            
            //TODO Set the sprite based on the Time T

            var index = Mathf.RoundToInt(useAnimationCurve
                ? Mathf.Lerp(0, _spriteCount, curve.Evaluate(t))
                : Mathf.Lerp(0, _spriteCount, t));
            
            

            targetRenderer.sprite = sprites[index];
        }
        
        //============================================================================================================//

        public void Play()
        {
            _playing = true;
        }

        public void Stop()
        {
            _playing = false;
            t = 0f;
            targetRenderer.sprite = null;
        }

        //============================================================================================================//
    }
}

