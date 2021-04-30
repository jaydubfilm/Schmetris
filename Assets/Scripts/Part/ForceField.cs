using System.Collections.Generic;
using StarSalvager.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class ForceField : Actor2DBase, ICanBeHit, IHealth
    {
        //IHealth Properties
        //====================================================================================================================//
        
        public float StartingHealth { get; private set; }
        public float CurrentHealth { get; private set; }

        //Properties
        //====================================================================================================================//
        
        [SerializeField]
        private Color disabledColor = Color.white;
        [FormerlySerializedAs("color")] [SerializeField]
        private Color defaultColor = Color.white;
        [SerializeField]
        private Color damageColor = Color.white;
        
        private PolygonCollider2D PolygonCollider2D
        {
            get
            {
                if (_polygonCollider2D == null)
                    _polygonCollider2D = GetComponent<PolygonCollider2D>();

                return _polygonCollider2D;
            }
        }

        private PolygonCollider2D _polygonCollider2D;
        
        public LineRenderer LineRenderer
        {
            get
            {
                if (_lineRenderer == null)
                    _lineRenderer = gameObject.GetComponent<LineRenderer>();

                return _lineRenderer;
            }
        }
        private LineRenderer _lineRenderer;

        private BotPartsLogic _botPartsLogic;
        
        private float[] _degrees;

        //====================================================================================================================//

        public void Init(in BotPartsLogic botPartsLogic, in float startRotDegrees, in float spreadDegrees, in float range)
        {
            Init(startRotDegrees, spreadDegrees, range);

            _botPartsLogic = botPartsLogic;
        }
        
        public void Init(in float startRotDegrees, in float spreadDegrees, in float range)
        {
            var deg = spreadDegrees / 4f;
            
            _degrees = new[]
            {
                deg* 2f + startRotDegrees,
                deg + startRotDegrees,
                startRotDegrees,
                
                startRotDegrees - deg,
                startRotDegrees - deg * 2f
            };
            
            SetPolygon(range);
            SetLineRenderer(range);

        }

        //Setup Components
        //====================================================================================================================//
        
        private void SetPolygon(in float range)
        {
            var polygonPositions = new List<Vector2>();
            //var currentPosition = Position;
            for (var i = 0; i < _degrees.Length; i++)
            {
                var degree = _degrees[i];
                polygonPositions.Add(Mathfx.GetAsPointOnCircle(degree, range +  0.25f));
            }
            for (var i = _degrees.Length - 1; i >= 0; i--)
            {
                var degree = _degrees[i];
                polygonPositions.Add(Mathfx.GetAsPointOnCircle(degree, range - 0.25f));
            }

            //PolygonCollider2D.offset = Vector2.one * (-range/2f);
            PolygonCollider2D.points = polygonPositions.ToArray();
        }
        
        private void SetLineRenderer(in float range)
        {
            LineRenderer.useWorldSpace = false;
            
            var positions = new Vector3[_degrees.Length];
            for (var i = 0; i < _degrees.Length; i++)
            {
                positions[i] = Mathfx.GetAsPointOnCircle(_degrees[i], range);
            }

            LineRenderer.positionCount = positions.Length;
            LineRenderer.SetPositions(positions);
            LineRenderer.widthMultiplier = 0.5f;
            SetColor(defaultColor);
        }

        //ICanBeHit Functions
        //====================================================================================================================//
        
        public bool TryHitAt(float damage)
        {
            _botPartsLogic.ResetForceFieldHealCooldown();
            ChangeHealth(-Mathf.Abs(damage));


            return true;
        }
        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            return TryHitAt(damage);
        }


        //IHealth Functions
        //====================================================================================================================//
        
        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
        }

        public void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (CurrentHealth <= 0)
            {
                SetColliderActive(false);
                SetColor(disabledColor);
                
            }
            else
            {
                SetColliderActive(true);
                SetColor(Color.Lerp(damageColor, defaultColor, CurrentHealth / StartingHealth));
            }
        }
        
        //Actor2DBase overrides
        //====================================================================================================================//
        

        public override void SetColor(Color color)
        {
            LineRenderer.endColor = LineRenderer.startColor = color;
        }

        public void SetColliderActive(in bool state)
        {
            PolygonCollider2D.enabled = state;
        }
        //====================================================================================================================//
        
    }
}
