using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using TMPro;
using UnityEngine;

namespace StarSalvager.Utilities
{
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText : MonoBehaviour, IRecycled, ICustomRecycle
    {
        //IRecycled Properties
        //====================================================================================================================//
        
        public bool IsRecycled { get; set; }

        //Properties
        //====================================================================================================================//
        
        private bool isReady;

        private TextMeshPro _text;

        private float _fadeTime;
        private float _t;
        private float _fadeRate;
        
        
        private float _waitTime;
        
        private float _floatSpeed;

        private Color _color;
        private Color _clearColor;

        private new Transform transform;

        //Unity Functions
        //====================================================================================================================//
        
        private void Start()
        {
            transform = gameObject.transform;
        }

        private void LateUpdate()
        {
            if (IsRecycled)
                return;

            if (!isReady)
                return;

            if (_waitTime > 0f)
            {
                _waitTime -= Time.deltaTime;
                return;
            }

            if (_fadeTime < 0f)
            {
                //isReady = false;
                Recycler.Recycle<FloatingText>(this);
                return;
            }
            
            transform.position += Vector3.up * (Time.deltaTime * _floatSpeed);

            _text.color = Color.Lerp(_color, _clearColor,  1f - (_fadeTime / _t));

            _fadeTime -= Time.deltaTime * _fadeRate;
        }

        //====================================================================================================================//
        public void Init(string text, Vector3 position, Color color)
        {
            Init(text, position, 0.75f, 1f, 5f, color);
        }
        public void Init(string text, Vector3 position, float waitTime, float fadeTime, float floatSpeed, Color color)
        {
            if (!_text)
                _text = GetComponent<TextMeshPro>();
            
            _t = _fadeTime = fadeTime;
            _fadeRate = 1.0f / fadeTime;
            
            
            _waitTime = waitTime;

            _floatSpeed = floatSpeed;

            _clearColor = _color = color;
            _clearColor.a = 0f;

            _text.color = _color;
            _text.text = text;

            transform.position = position;

            isReady = true;

        }

        //ICustomRecycle Functions
        //====================================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            isReady = false;
        }


        //====================================================================================================================//

        public static void Create(string text, Vector3 position, Color color)
        {
            FactoryManager.Instance?.GetFactory<ParticleFactory>().CreateObject<FloatingText>()
                .Init(text, position, color);
        }

        //Unity Editor Functions
        //====================================================================================================================//

#if UNITY_EDITOR
        [Button, DisableInEditorMode]
        private void Test()
        {
            if (!transform)
                transform = gameObject.transform;
            
            Init("+98765", transform.position, 0.75f,1f,5f, Color.green);
        }
#endif
    }
}
