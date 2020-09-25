using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.Factories;
using UnityEngine;

namespace StarSalvager.Utilities.Particles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ConnectedSpriteObject : ConnectedObject
    {
        [SerializeField]
        private float fadeTime;
        private float _startFadeTime;
        
        private Dictionary<SpriteRenderer, Color> _renderers;

        //====================================================================================================================//
        
        protected override void Start()
        {
            base.Start();

            _startFadeTime = fadeTime;
            
            _renderers = new Dictionary<SpriteRenderer, Color>();
            
            //Get all sprite Renderers
            var renderers = new List<SpriteRenderer>(GetComponents<SpriteRenderer>());
            renderers.AddRange(GetComponentsInChildren<SpriteRenderer>());

            foreach (var spriteRenderer in renderers.Where(spriteRenderer => !_renderers.ContainsKey(spriteRenderer)))
            {
                _renderers.Add(spriteRenderer, spriteRenderer.color);
            }
            
        }

        protected override void LateUpdate()
        {
            if (!_isReady || IsRecycled)
                return;
            
            if (_renderers == null || _renderers.Count == 0)
                return;
            
            transform.position = _offset + _connectedTransform.position;

            if (lifeTime > 0f)
            {
                lifeTime -= Time.deltaTime;
                return;
            }


            if (fadeTime > 0)
            {
                fadeTime -= Time.deltaTime;

                foreach (var pair in _renderers)
                {
                    var (renderer, color) = (pair.Key, pair.Value);

                    renderer.color = Color.Lerp(color, Color.clear, 1f - fadeTime / _startFadeTime);
                }

                return;
            }
            
            Recycler.Recycle<ConnectedSpriteObject>(this);
            
        }

        //====================================================================================================================//
        public override void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);
            
            fadeTime = _startFadeTime;
            
            foreach (var pair in _renderers)
            {
                var (renderer, color) = (pair.Key, pair.Value);

                renderer.color = color;
            }
            
        }

        //====================================================================================================================//
        
        public static void Create(Transform connectedTransform, Vector3 offset)
        {
            if (FactoryManager.Instance == null)
                return;
            
            FactoryManager.Instance.GetFactory<ParticleFactory>().CreateObject<ConnectedSpriteObject>()
                .Init(connectedTransform, offset);
        }

        //====================================================================================================================//
        
    }
}
