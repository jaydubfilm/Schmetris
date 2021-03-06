﻿using System;
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
            TRAIL,
            BONUS_SHAPE,
            BONUS_SHAPE_PARTICLE,
            BIT_DEATH,
            CURVE_LINE
        }

        public enum PART_EFFECT
        {
            REPAIR,
            REFINER,
            RATE_BOOST,
            GUN,
            TRIPLE_SHOT,
            BOMB,
            FREEZE,
            SHIELD
        }
        
        private readonly EffectProfileScriptableObject _effectProfileScriptableObject;

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            return gameObject.GetComponent<T>();
        }

        public GameObject CreateEffect(EFFECT effect, BIT_TYPE bitType)
        {
            GameObject gameObject;
            switch (effect)
            {
                case EFFECT.BIT_DEATH:

                    switch (bitType)
                    {
                        case BIT_TYPE.BLUE:
                            gameObject = Object.Instantiate(_effectProfileScriptableObject.bitBlueParticlePrefab);
                            break;
                        case BIT_TYPE.GREEN:
                            gameObject = Object.Instantiate(_effectProfileScriptableObject.bitGreenParticlePrefab);
                            break;
                        case BIT_TYPE.GREY:
                            gameObject = Object.Instantiate(_effectProfileScriptableObject.bitGreyParticlePrefab);
                            break;
                        case BIT_TYPE.RED:
                            gameObject = Object.Instantiate(_effectProfileScriptableObject.bitRedParticlePrefab);
                            break;
                        case BIT_TYPE.YELLOW:
                            gameObject = Object.Instantiate(_effectProfileScriptableObject.bitYellowParticlePrefab);
                            break;
                        case BIT_TYPE.WHITE:
                            gameObject = Object.Instantiate(_effectProfileScriptableObject.bitWhiteParticlePrefab);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bitType), bitType, null);
                    }
                    
                    break;
                default:
                    gameObject = CreateEffect(effect);
                    break;
            }

            return gameObject;
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
                case EFFECT.BONUS_SHAPE:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.bonusShapeEffectPrefab);
                    break;
                case EFFECT.BONUS_SHAPE_PARTICLE:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.bonusShapeParticlesPrefab);
                    break;
                case EFFECT.CURVE_LINE:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.blasterLineEffectPrefab);
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
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.bombShockwaveEffectPrefab);
                    break;
                case PART_EFFECT.FREEZE:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.freezeShockwaveEffectPrefab);
                    break;
                case PART_EFFECT.SHIELD:
                    gameObject = Object.Instantiate(_effectProfileScriptableObject.shieldEffectPrefab);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(partEffect), partEffect, null);
            }
            
            return gameObject;
        }

        public SpriteRenderer CreateSimpleSpriteRenderer()
        {
            return Object.Instantiate(_effectProfileScriptableObject.simpleSpritePrefab).GetComponent<SpriteRenderer>();
        }

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


