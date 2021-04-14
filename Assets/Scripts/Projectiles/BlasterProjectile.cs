using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager.Projectiles
{
    [RequireComponent(typeof(LineRenderer))]
    public class BlasterProjectile : MonoBehaviour
    {
        //Properties
        //====================================================================================================================//

        //[SerializeField] private int segments;

        [SerializeField] private AnimationCurve lineRangeCurve;
        [SerializeField] private AnimationCurve lineSizeCurve;
        [SerializeField] private AnimationCurve lineColorCurve;
        
        [SerializeField] private float lineTargetSize;
        [SerializeField] private Color lineTargetColor = Color.white;

        private LineRenderer _lineRenderer;
        private new Transform transform;

        private float[] _degrees;

        //Unity Functions
        //====================================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {

        }

        //Blaster Projectile Functions
        //====================================================================================================================//

        public void Init(in float startDegrees, in float degreesOffset, in float range, in float fireTime)
        {
            transform = gameObject.transform;
            _lineRenderer = GetComponent<LineRenderer>();

            var deg = degreesOffset / 4f;
            
            _degrees = new[]
            {
                deg* 2f + startDegrees,
                deg + startDegrees,
                startDegrees,
                
                startDegrees - deg,
                startDegrees - deg * 2f
            };

            _lineRenderer.positionCount = _degrees.Length;
            StartCoroutine(LineGrowCoroutine(fireTime, range));
        }

        private IEnumerator LineGrowCoroutine(float shootTime, float targetRange)
        {
            var startPosition = transform.position;

            Color color;
            float range;
            float scale;
            var positions = new Vector3[_degrees.Length];

            //--------------------------------------------------------------------------------------------------------//
            var t = 0f;

            while (t / shootTime < 1f)
            {
                var td = t / shootTime;

                range = Mathf.Lerp(0, targetRange, lineRangeCurve.Evaluate(td));
                scale = Mathf.Lerp(0, lineTargetSize, lineSizeCurve.Evaluate(td));
                color = Color.Lerp(Color.white, lineTargetColor, lineColorCurve.Evaluate(td));


                for (var i = 0; i < _degrees.Length; i++)
                {
                    positions[i] = startPosition + (Vector3) Mathfx.GetAsPoint(_degrees[i], range);
                }

                _lineRenderer.SetPositions(positions);
                _lineRenderer.widthMultiplier = scale;
                _lineRenderer.startColor = _lineRenderer.endColor = color;

                t += Time.deltaTime;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
