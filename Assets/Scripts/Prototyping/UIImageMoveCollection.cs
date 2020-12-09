using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace StarSalvager.Prototype
{
    public class UIImageMoveCollection : MonoBehaviour
    {
        //[SerializeField] private Transform mainTransform;

        [SerializeField] private Transform[] rotationTransforms;
        [SerializeField] private Transform[] wobbleTransforms;
        

        [SerializeField, MinMaxSlider(0, 100, true)]
        private Vector2 speedRange;
        
        [SerializeField, MinMaxSlider(0, 100, true)]
        private Vector2 wobbleRange;
        
        [SerializeField, MinMaxSlider(0, 10, true)]
        private Vector2 wobbleSpeedRange;

        [SerializeField]
        private AnimationCurve wobbleCurve;

        private float[] _rotationSpeeds;
        private float[] _wobbleRanges;
        private float[] _wobbleSpeeds;

        private bool _ready;


        //====================================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            SetupRotations();
            SetupWobbles();
            
            _ready = true;
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
            if (rotationTransforms.IsNullOrEmpty())
                return;

            foreach (var affectedTransform in rotationTransforms)
            {
                affectedTransform.gameObject.name = affectedTransform.GetComponent<Image>().sprite.name;
            }
        }

#endif

        //====================================================================================================================//

        [Button, DisableInEditorMode]
        private void SetupRotations()
        {
            if (rotationTransforms.IsNullOrEmpty())
            {
                throw new Exception("No transforms to setup");
            }

            _rotationSpeeds = new float[rotationTransforms.Length];
            for (int i = 0; i < _rotationSpeeds.Length; i++)
            {
                _rotationSpeeds[i] = Random.Range(speedRange.x, speedRange.y) * (Random.value > 0.5f ? -1f : 1f);
            }

            
        }

        private void SetupWobbles()
        {
            if (wobbleTransforms.IsNullOrEmpty())
            {
                throw new Exception("No transforms to setup");
            }

            _wobbleRanges = new float[wobbleTransforms.Length];
            _wobbleSpeeds = new float[wobbleTransforms.Length];
            for (int i = 0; i < _wobbleRanges.Length; i++)
            {
                _wobbleRanges[i] = Random.Range(wobbleRange.x, wobbleRange.y);
                _wobbleSpeeds[i] = Random.Range(wobbleSpeedRange.x, wobbleSpeedRange.y);
            }
        }

        //====================================================================================================================//


        private void MoveTransforms()
        {
            for (int i = 0; i < _rotationSpeeds.Length; i++)
            {
                var eulerAngles = rotationTransforms[i].eulerAngles;

                eulerAngles += Vector3.forward * (_rotationSpeeds[i] * Time.deltaTime);

                rotationTransforms[i].eulerAngles = eulerAngles;
            }

            for (int i = 0; i < _wobbleRanges.Length; i++)
            {
                var eulerAngles = wobbleTransforms[i].eulerAngles;

                var td = wobbleCurve.Evaluate(Mathf.PingPong(Time.time, _wobbleSpeeds[i]) / _wobbleSpeeds[i]);

                eulerAngles.z = Mathf.Lerp(
                    -_wobbleRanges[i],
                    _wobbleRanges[i],
                    td);

                wobbleTransforms[i].eulerAngles = eulerAngles;
            }
        }
    }

}