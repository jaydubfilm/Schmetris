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
            THRUST,
            EXPLOSION,
            MERGE,
            LINE,
            TRAIL
        }

        public enum PART_EFFECT
        {
            REPAIR,
            REFINER,
            RATE_BOOST,
            GUN,
            TRIPLE_SHOT,
            BOMB,
            FREEZE
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
                /*case bool _ when type == typeof(Explosion):
                    gameObject = CreateExplosion();
                    break;*/
                case bool _ when type == typeof(TextMeshPro):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.labelPrefab);//CreateLabel();
                    break;
                case bool _ when type == typeof(FloatingText):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.floatingTextPrefab);
                    break;
                case bool _ when type == typeof(ConnectedSpriteObject):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.magnetIconSpritePrefab);
                    break;
                case bool _ when type == typeof(FadeSprite):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.fadeSpritePrefab);
                    break;
                case bool _ when type == typeof(LineShrink):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.lineShrinkPrefab);
                    break;
                case bool _ when type == typeof(FlashSprite):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.alertIconPrefab);
                    break;
                case bool _ when type == typeof(Damage):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.damageEffectPrefab);
                    break;
                case bool _ when type == typeof(Shield):
                    gameObject = CreateObject<T>(_effectProfileScriptableObject.shieldPrototypePrefab);
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
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.weldEffectPrefab);
                    break;
                case EFFECT.IMPACT:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.impactEffectPrefab);
                    break;
                case EFFECT.THRUST:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.thrustEffectPrefab);
                    break;
                case EFFECT.EXPLOSION:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.explosionEffectPrefab);
                    break;
                case EFFECT.MERGE:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.mergeEffectPrefab);
                    break;
                case EFFECT.LINE:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.lineRendererPrefab);
                    break;
                case EFFECT.TRAIL:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.trailRendererPrefab);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
            }
            
            return gameObject;
        }
        
        public GameObject CreatePartEffect(PART_EFFECT partEffect)
        {
            GameObject gameObject;
            
            switch (partEffect)
            {
                case PART_EFFECT.REPAIR:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.repairEffectPrefab);
                    break;
                case PART_EFFECT.REFINER:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.refinerEffectPrefab);
                    break;
                case PART_EFFECT.RATE_BOOST:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.boostRateEffectPrefab);
                    break;
                case PART_EFFECT.GUN:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.gunEffectPrefab);
                    break;
                case PART_EFFECT.TRIPLE_SHOT:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.gunEffectPrefab);
                    gameObject.GetComponent<SpriteRenderer>().sprite =
                        _effectProfileScriptableObject.tripleTurretSprite;
                    break;
                case PART_EFFECT.BOMB:
                case PART_EFFECT.FREEZE:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.shockwaveEffectPrefab);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(partEffect), partEffect, null);
            }
            
            return gameObject;
        }

        //Create Specific Prefabs
        //============================================================================================================//

        /*private GameObject CreateDamage()
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
            throw new NotImplementedException();
            /*if (!Recycler.TryGrab<Explosion>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_effectProfileScriptableObject.explosionPrefab);
            }

            return gameObject;#1#
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
        }*/

        //====================================================================================================================//

        private static GameObject CreateObject<T>(GameObject prefab)
        {
            if (!Recycler.TryGrab<T>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(prefab);
            }

            return gameObject;
        }
        
    }
}


