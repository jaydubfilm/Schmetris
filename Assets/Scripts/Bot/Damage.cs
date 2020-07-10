using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteMask))]
    public class Damage : MonoBehaviour, IRecycled, ICustomRecycle
    {
        //============================================================================================================//
        
        public bool IsRecycled { get; set; }
        
        private new SpriteRenderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponent<SpriteRenderer>();
                
                return _renderer;
            }
        }
        private SpriteRenderer _renderer;

        private SpriteMask mask
        {
            get
            {
                if (_mask == null)
                    _mask = GetComponent<SpriteMask>();

                return _mask;
            }
        }
        private SpriteMask _mask;
        
        public new Transform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.transform;

                return _transform;
            }
        }
        private Transform _transform;

        [SerializeField, Required]
        private DamageProfileScriptableObject _damageProfileScriptableObject;

        [SerializeField] 
        private SimpleAnimator flashAnimator;

        //============================================================================================================//

        public void SetHealth(float value)
        {
            
            if (value == 1f)
            {
                flashAnimator.Stop();
                renderer.sprite = null;
                mask.sprite = null;
                return;
            }
            
            flashAnimator.Play();
            
            renderer.sprite = _damageProfileScriptableObject.GetDetailSprite(value);
            mask.sprite = _damageProfileScriptableObject.GetMaskSprite(value);

            flashAnimator.speed = Mathf.Lerp(1.5f, 5f, 1f - value);
            flashAnimator.Alpha = Mathf.Lerp(0.25f, 1f, 1f - value);
        }
        
        //============================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            renderer.sprite = null;
            mask.sprite = null;
        }
    }
}

