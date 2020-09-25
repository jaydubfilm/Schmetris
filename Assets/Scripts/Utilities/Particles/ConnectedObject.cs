using System.Collections;
using System.Collections.Generic;
using Recycling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Particles
{
    public class ConnectedObject : MonoBehaviour, IRecycled, ICustomRecycle
    {
        [SerializeField]
        private float lifeTime;

        [ShowInInspector, ReadOnly]
        private Vector3 _offset;
        private Transform connectedTransform;
        
        private new Transform transform;
        
        public bool IsRecycled { get; set; }


        //====================================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            transform = gameObject.transform;
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if (!connectedTransform)
                return;
        }

        //====================================================================================================================//
        
        //public void Init()
        
        //ICustomRecycle Functions
        //====================================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            connectedTransform = null;
        }

        //====================================================================================================================//
        
    }

}