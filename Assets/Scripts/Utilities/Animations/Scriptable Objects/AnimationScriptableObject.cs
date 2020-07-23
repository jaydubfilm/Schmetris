using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    [CreateAssetMenu(fileName = "Animation", menuName = "Star Salvager/Scriptable Objects/Simple Animation")]
    public class AnimationScriptableObject : ScriptableObject
    {
        //============================================================================================================//
        
        [SerializeField]
        private Sprite[] sprites;

        [SerializeField, Range(1f, 10f)]
        private float speed;

        [SerializeField]
        private bool PingPong;

        [SerializeField, ToggleGroup("useAnimationCurve")]
        private bool useAnimationCurve;
        
        [SerializeField, ToggleGroup("useAnimationCurve")]
        private AnimationCurve curve = new AnimationCurve();

        [NonSerialized]
        private bool _setup;

        private int spriteCount
        {
            get
            {
                if (_setup) return _spriteCount;

                _spriteCount = sprites.Length - 1;
                _setup = true;
                
                return _spriteCount;
            }
        }
        [NonSerialized]
        private int _spriteCount;
        
        //============================================================================================================//

        public Sprite PlayFrame(float speedMult, ref float t)
        {
            
            if (PingPong)
            {
                var value = 1f / (speed * speedMult);

                t = Mathf.PingPong(Time.time, value) / value;
            }
            else
            {
                if (t >= 1f)
                    t = 0f;

                t += Time.deltaTime * speed * speedMult;
            }

            
            //Set the sprite based on the Time T
            var index = useAnimationCurve
                ? GetIndex(curve.Evaluate(t), spriteCount) /* _spriteCount)Mathf.Lerp(0, _spriteCount, )*/
                : GetIndex(t, spriteCount);
            

            return sprites[index];
        }

        private static int GetIndex(float t, int count)
        {
            return (int) (count * t);
        }
        
        //============================================================================================================//
    }
}

