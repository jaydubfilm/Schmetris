using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.Prototype
{
    public class SpriteMoveCollection : MonoBehaviour
    {

        [SerializeField] private Transform mainTransform;

        [SerializeField] private Transform[] affectedTransforms;

        [SerializeField, MinMaxSlider(0, 1000, true)]
        private Vector2 speedRange;

        private float[] _rotationSpeeds;

        private bool _ready;


        //====================================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            SetupRotations();
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if (!_ready)
                return;

            MoveTransforms();
        }
#if UNITY_EDITOR

        private void OnValidate()
        {
            if (affectedTransforms.IsNullOrEmpty())
                return;
            
            foreach (var affectedTransform in affectedTransforms)
            {
                affectedTransform.gameObject.name = affectedTransform.GetComponent<SpriteRenderer>().sprite.name;
            }
        }

#endif

        //====================================================================================================================//

        [Button, DisableInEditorMode]
        private void SetupRotations()
        {
            if (affectedTransforms.IsNullOrEmpty())
            {
                throw new Exception("No transforms to setup");
            }

            _rotationSpeeds = new float[affectedTransforms.Length];
            for (int i = 0; i < _rotationSpeeds.Length; i++)
            {
                _rotationSpeeds[i] = Random.Range(speedRange.x, speedRange.y) * (Random.value > 0.5f ? -1f : 1f);
            }

            _ready = true;
        }

        private void MoveTransforms()
        {
            for (int i = 0; i < _rotationSpeeds.Length; i++)
            {
                var transform = affectedTransforms[i];
                var dirToMain = (mainTransform.position - transform.position).normalized;

                transform.position -= dirToMain * (0.01f * Time.deltaTime);
                
                
                var eulerAngles = affectedTransforms[i].eulerAngles;
                
                eulerAngles += Vector3.forward * (_rotationSpeeds[i] * Time.deltaTime);

                affectedTransforms[i].eulerAngles = eulerAngles;
            }
        }
    }

}