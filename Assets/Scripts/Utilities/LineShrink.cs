using System;
using Recycling;
using UnityEngine;

namespace StarSalvager.Utilities
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineShrink : Actor2DBase, ICustomRecycle
    {
        [SerializeField] private AnimationCurve shrinkCurve;

        //[SerializeField] private float shrinkTime;

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



        private float _shrinkTime;
        private float _startWidth;
        private float _t;
        private bool _ready;

        //Unity Functions
        //====================================================================================================================//

        private void Update()
        {
            if (!_ready)
                return;

            if (_t >= _shrinkTime)
            {
                _ready = false;
                Recycler.Recycle<LineShrink>(this);
                return;
            }

            renderer.widthMultiplier = _startWidth * shrinkCurve.Evaluate(_t / _shrinkTime);

            _t += Time.deltaTime;
        }

        //LineShrink Functions
        //====================================================================================================================//

        public void Init(in Vector3 startPosition, 
            in Vector3 endPosition, 
            in float startWidth = 0.5f,
            in float shrinkTimer = 0.7f)
        {
            _shrinkTime = shrinkTimer;
            
            _startWidth = startWidth;
            renderer.widthMultiplier = _startWidth;
            renderer.SetPositions(new[]
            {
                startPosition,
                endPosition
            });

            _ready = true;
        }

        //Actor2DBase
        //====================================================================================================================//

        public override void SetColor(Color color)
        {
            renderer.endColor = renderer.startColor = color;
        }

        public override void SetSprite(Sprite sprite)
        {
            throw new NotImplementedException();
        }

        //ICustomRotate Functions
        //====================================================================================================================//

        public void CustomRecycle(params object[] args)
        {
            _ready = false;
            _t = 0f;
            renderer.SetPositions(new[]
            {
                Vector3.zero,
                Vector3.zero
            });
        }
    }
}
