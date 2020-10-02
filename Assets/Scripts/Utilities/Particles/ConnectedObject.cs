using System;
using System.Collections;
using System.Collections.Generic;
using Recycling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Particles
{
    public class ConnectedObject : MonoBehaviour, IRecycled, ICustomRecycle
    {
        public bool IsRecycled { get; set; }

        //====================================================================================================================//
        
        [SerializeField]
        protected float lifeTime;
        private float _startLifetime;

        [ShowInInspector, ReadOnly]
        protected Vector3 _offset;
        protected Transform _connectedTransform;

        protected new Transform transform => _transform ? _transform : _transform = gameObject.transform;
        private Transform _transform;

        protected bool _isReady;

        //====================================================================================================================//
        
        // Start is called before the first frame update
        protected virtual void Start()
        {
            _startLifetime = lifeTime;
        }

        // Update is called once per frame
        protected virtual void LateUpdate()
        {
            if (!_isReady)
                return;

            transform.position = _offset + _connectedTransform.position;
        }

        //====================================================================================================================//

        public virtual void Init(Transform connectedTransform, Vector3 offset)
        {
            _offset = offset;
            _connectedTransform = connectedTransform ? connectedTransform : throw new NullReferenceException();
            
            transform.position = _offset + _connectedTransform.position;
            
            _isReady = true;
        }
        
        //ICustomRecycle Functions
        //====================================================================================================================//
        
        public virtual void CustomRecycle(params object[] args)
        {
            _connectedTransform = null;
            _isReady = false;
            _offset = Vector3.zero;
            
            lifeTime = _startLifetime;
        }

        //====================================================================================================================//
        
    }

}