using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
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

        //============================================================================================================//

        public void SetHealth(float value)
        {
            if (value == 1f)
            {
                renderer.sprite = null;
                mask.sprite = null;
                return;
            }
            
            renderer.sprite = _damageProfileScriptableObject.GetDetailSprite(value);
            mask.sprite = _damageProfileScriptableObject.GetMaskSprite(value);
        }
        
        //============================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            renderer.sprite = null;
            mask.sprite = null;
        }
    }
}

