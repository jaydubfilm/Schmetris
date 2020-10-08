﻿using System;
using Recycling;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities.Particles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    public class ParticleFactory : FactoryBase
    {
        private readonly GameObject _explosionPrefab;
        private readonly GameObject _labelPrefab;
        private readonly GameObject _floatingTextPrefab;
        private readonly GameObject _connectedSpritePrefab;
        private readonly GameObject _fadeSpritePrefab;
        private readonly GameObject _shrinkLinePrefab;
        
        //============================================================================================================//

        public ParticleFactory(GameObject explosionPrefab, GameObject labelPrefab, GameObject floatingTextPrefab, GameObject connectedSpritePrefab, GameObject fadeSpritePrefab, GameObject shrinkLinePrefab)
        {
            _explosionPrefab = explosionPrefab;
            _labelPrefab = labelPrefab;
            _floatingTextPrefab = floatingTextPrefab;
            _connectedSpritePrefab = connectedSpritePrefab;
            _fadeSpritePrefab = fadeSpritePrefab;

            _shrinkLinePrefab = shrinkLinePrefab;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            return gameObject.GetComponent<T>();
        }
        
        //============================================================================================================//

        private GameObject CreateExplosion()
        {
            if (!Recycler.TryGrab<Explosion>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_explosionPrefab);
            }

            return gameObject;
        }
        
        private GameObject CreateLabel()
        {
            if (!Recycler.TryGrab<TextMeshPro>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_labelPrefab);
            }

            return gameObject;
        }

        private GameObject CreateFloatingText()
        {
            if (!Recycler.TryGrab<FloatingText>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_floatingTextPrefab);
            }

            return gameObject;
        }
        private GameObject CreateConnectedSprite()
        {
            if (!Recycler.TryGrab<ConnectedSpriteObject>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_connectedSpritePrefab);
            }

            return gameObject;
        }
        private GameObject CreateFadeSprite()
        {
            if (!Recycler.TryGrab<FadeSprite>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_fadeSpritePrefab);
            }

            return gameObject;
        }
        
        private GameObject CreateLineShrink()
        {
            if (!Recycler.TryGrab<LineShrink>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_shrinkLinePrefab);
            }

            return gameObject;
        }
        
    }
}


