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
        
        protected new Transform transform;

        protected bool _isReady;

        //====================================================================================================================//
        
        // Start is called before the first frame update
        protected virtual void Start()
        {
            transform = gameObject.transform;
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

            _isReady = true;
        }
        
        //ICustomRecycle Functions
        //====================================================================================================================//
        
        public virtual void CustomRecycle(params object[] args)
        {
            _connectedTransform = null;
            _isReady = false;
            lifeTime = _startLifetime;
        }

        //====================================================================================================================//
        
    }

}