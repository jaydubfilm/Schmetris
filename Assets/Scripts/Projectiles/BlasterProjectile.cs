using System;
using System.Collections;
using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager.Projectiles
{
    [RequireComponent(typeof(LineRenderer))]
    public class BlasterProjectile : Actor2DBase
    {
        //Properties
        //====================================================================================================================//

        [SerializeField] private AnimationCurve lineRangeCurve;
        [SerializeField] private AnimationCurve lineSizeCurve;
        [SerializeField] private AnimationCurve lineColorCurve;
        
        [SerializeField] private float lineTargetSize;
        [SerializeField] private Color lineTargetColor = Color.white;
        
        private float[] _degrees;


        public new LineRenderer renderer
        {
            get
            {
                if (_lineRenderer == null)
                    _lineRenderer = gameObject.GetComponent<LineRenderer>();

                return _lineRenderer;
            }
        }
        private LineRenderer _lineRenderer;


        //Blaster Projectile Functions
        //====================================================================================================================//

        public void Init(in float startDegrees, in float degreesOffset, in float range, in float fireTime)
        {
            var deg = degreesOffset / 4f;
            
            _degrees = new[]
            {
                deg* 2f + startDegrees,
                deg + startDegrees,
                startDegrees,
                
                startDegrees - deg,
                startDegrees - deg * 2f
            };

            
            renderer.positionCount = _degrees.Length;
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
                    positions[i] = startPosition + (Vector3) Mathfx.GetAsPointOnCircle(_degrees[i], range);
                }

                renderer.SetPositions(positions);
                renderer.widthMultiplier = scale;
                SetColor(color);

                t += Time.deltaTime;

                yield return null;
            }

            Destroy(gameObject);
        }

        //Actor2DBase Overrides
        //====================================================================================================================//
        public override void SetColor(Color color)
        {
            renderer.startColor = renderer.endColor = color;
        }

        public override void SetSprite(Sprite sprite) => throw new NotImplementedException();

        //====================================================================================================================//
        
    }
}
