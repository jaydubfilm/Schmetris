using System;
using Recycling;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities.Particles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    public class EffectFactory : FactoryBase
    {
        public enum EFFECT
        {
            WELD,
            IMPACT,
            THRUST
        }
        
        private readonly EffectProfileScriptableObject _effectProfileScriptableObject;
        /*private readonly GameObject _explosionPrefab;
        private readonly GameObject _labelPrefab;
        private readonly GameObject _floatingTextPrefab;
        private readonly GameObject _connectedSpritePrefab;
        private readonly GameObject _fadeSpritePrefab;
        private readonly GameObject _shrinkLinePrefab;*/
        
        //============================================================================================================//

        /*public EffectFactory(GameObject explosionPrefab, GameObject labelPrefab, GameObject floatingTextPrefab, GameObject connectedSpritePrefab, GameObject fadeSpritePrefab, GameObject shrinkLinePrefab)
        {
            _explosionPrefab = explosionPrefab;
            _labelPrefab = labelPrefab;
            _floatingTextPrefab = floatingTextPrefab;
            _connectedSpritePrefab = connectedSpritePrefab;
            _fadeSpritePrefab = fadeSpritePrefab;

            _shrinkLinePrefab = shrinkLinePrefab;
        }*/

        public EffectFactory(EffectProfileScriptableObject effectProfileScriptableObject)
        {
            _effectProfileScriptableObject = effectProfileScriptableObject;
        }
        
        //============================================================================================================//
        
        public override GameObject CreateGameObject()
        {
            throw new NotImplementedException();
        }

        //The intention would be to create multiple types of particles here
        public override T CreateObject<T>()
        {
            GameObject gameObject;
            
            var type = typeof(T);
            switch (true)
            {
                case bool _ when type == typeof(Explosion):
                    gameObject = CreateExplosion();
                    break;
                case bool _ when type == typeof(TextMeshPro):
                    gameObject = CreateLabel();
                    break;
                case bool _ when type == typeof(FloatingText):
                    gameObject = CreateFloatingText();
                    break;
                case bool _ when type == typeof(ConnectedSpriteObject):
                    gameObject = CreateConnectedSprite();
                    break;
                case bool _ when type == typeof(FadeSprite):
                    gameObject = CreateFadeSprite();
                    break;
                case bool _ when type == typeof(LineShrink):
                    gameObject = CreateLineShrink();
                    break;
                case bool _ when type == typeof(FlashSprite):
                    gameObject = CreateAlert();
                    break;
                case bool _ when type == typeof(Damage):
                    gameObject = CreateDamage();
                    break;
                case bool _ when type == typeof(Shield):
                    gameObject = CreateShield();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            return gameObject.GetComponent<T>();
        }

        
        public GameObject CreateEffect(EFFECT effect)
        {
            GameObject gameObject;
            
            switch (effect)
            {
                case EFFECT.WELD:
                    gameObject = CreateWeldEffect();
                    break;
                case EFFECT.IMPACT:
                    gameObject = CreateImpactEffect();
                    break;
                case EFFECT.THRUST:
                    gameObject = CreateThrustEffect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
            }
            
            return gameObject;
        }

        //Create Effects Prefabs
        //====================================================================================================================//
        
        private GameObject CreateWeldEffect()
        {
            return Object.Instantiate(_effectProfileScriptableObject.weldEffectPrefab);
        }
        
        private GameObject CreateImpactEffect()
        {
            return Object.Instantiate(_effectProfileScriptableObject.impactEffectPrefab);
        }
        
        private GameObject CreateThrustEffect()
        {
            return Object.Instantiate(_effectProfileScriptableObject.thrustEffectPrefab);
        }
        
        
        //Create Specific Prefabs
        //============================================================================================================//

        private GameObject CreateDamage()
        {
            if (!Recycler.TryGrab<Damage>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.damageEffectPrefab);
            }

            return gameObject;
        }
        
        private GameObject CreateShield()
        {
            if (!Recycler.TryGrab<Shield>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.shieldPrototypePrefab);
            }

            return gameObject;
        }

        private GameObject CreateExplosion()
        {
            if (!Recycler.TryGrab<Explosion>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.explosionPrefab);
            }

            return gameObject;
        }
        
        private GameObject CreateAlert()
        {
            if (!Recycler.TryGrab<FlashSprite>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.alertIconPrefab);
            }

            return gameObject;
        }
        
        private GameObject CreateLabel()
        {
            if (!Recycler.TryGrab<TextMeshPro>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.labelPrefab);
            }

            return gameObject;
        }

        private GameObject CreateFloatingText()
        {
            if (!Recycler.TryGrab<FloatingText>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.floatingTextPrefab);
            }

            return gameObject;
        }
        private GameObject CreateConnectedSprite()
        {
            if (!Recycler.TryGrab<ConnectedSpriteObject>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.magnetIconSpritePrefab);
            }

            return gameObject;
        }
        private GameObject CreateFadeSprite()
        {
            if (!Recycler.TryGrab<FadeSprite>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.fadeSpritePrefab);
            }

            return gameObject;
        }
        
        private GameObject CreateLineShrink()
        {
            if (!Recycler.TryGrab<LineShrink>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.lineShrinkPrefab);
            }

            return gameObject;
        }

        //====================================================================================================================//
        
    }
}


