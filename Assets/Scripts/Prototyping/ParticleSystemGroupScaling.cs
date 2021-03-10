using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Prototype
{
    public class ParticleSystemGroupScaling : MonoBehaviour
    {
        [Serializable]
        public struct ParticleData
        {
            [FoldoutGroup("$GetName")]
            public ParticleSystem ParticleSystem;

            private string GetName()
            {
                return ParticleSystem ? ParticleSystem.gameObject.name : string.Empty;
            }

        }

        [ShowInInspector]
        public float AnimationTime => GetMaxLifeTime() / simulationSpeed;


        [SerializeField]
        private ParticleData[] particleDatas;

        [SerializeField, Range(0.001f, 2f), OnValueChanged("UpdateSimulationSpeed")]
        private float simulationSpeed = 1f;
        
        [SerializeField, Range(0.1f, 10f), OnValueChanged("UpdateSimulationSize")]
        private float simulationScale = 1f;

        [SerializeField, OnValueChanged("UpdateSimulationSize")]
        private float scalingMultiplier = 1f;

        private new Transform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.transform;

                return _transform;
            }
        }
        private Transform _transform;

        //Unity Functions
        //====================================================================================================================//
        
        /*// Start is called before the first frame update
        void Start()
        {

        }*/


        //ParticleSizing Functions
        //====================================================================================================================//

        public void SetSimulationSize(float size)
        {
            transform.localScale = Vector3.one * (size * scalingMultiplier);
        }
        
        public void SetSimulationSpeed(float speed)
        {
            if (particleDatas.IsNullOrEmpty())
                return;

            speed = Mathf.Clamp(speed, 0.001f, 2f);
            foreach (var particleData in particleDatas)
            {
                var main = particleData.ParticleSystem.main;
                
                main.simulationSpeed = speed;
            }
        }

        private float GetMaxLifeTime()
        {
            if (particleDatas.IsNullOrEmpty())
                return default;
            
            var life = 0f;
            foreach (var particleData in particleDatas)
            {
                life = Mathf.Max(life, particleData.ParticleSystem.main.startLifetime.constant);
            }

            return life;
        }

#if UNITY_EDITOR
        private void UpdateSimulationSpeed()
        {
            SetSimulationSpeed(simulationSpeed);
        }
        private void UpdateSimulationSize()
        {
            SetSimulationSize(simulationScale);
        }

        [Button]
        private void PopulateList()
        {
            var particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
            
            particleDatas = new ParticleData[particleSystems.Length];

            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleDatas[i] = new ParticleData
                {
                    ParticleSystem =  particleSystems[i]
                };
            }
            
        }
#endif


        //====================================================================================================================//
        
    }
}
