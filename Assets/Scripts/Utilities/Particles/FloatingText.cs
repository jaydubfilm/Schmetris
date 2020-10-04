using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using TMPro;
using UnityEngine;

namespace StarSalvager.Utilities.Particles
{
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText : MonoBehaviour, IRecycled, ICustomRecycle
    {
        //IRecycled Properties
        //====================================================================================================================//
        
        public bool IsRecycled { get; set; }


        //====================================================================================================================//

        [Range(0f, 3f), LabelText("Time till Fade"), DisableInNonPrefabs]
        public float waitTime = 0.75f;
        [Range(0.01f, 3f), LabelText("Time to Fade out"), DisableInNonPrefabs]
        public float fadeTime = 1f;
        [Range(0f, 10f), LabelText("Rise Speed"), DisableInNonPrefabs]
        public float floatSpeed = 5;

        //Properties
        //====================================================================================================================//
        
        private bool isReady;

        private float _fadeTime;
        private float _t;
        private float _fadeRate;
        
        
        private float _waitTime;
        
        private float _floatSpeed;

        private Color _color;
        private Color _clearColor;

        //====================================================================================================================//
        
        private TextMeshPro TextMeshPro
        {
            get
            {
                if (_text == null)
                    _text = GetComponent<TextMeshPro>();

                return _text;
            }
        }
        private TextMeshPro _text;

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

            TextMeshPro.color = Color.Lerp(_color, _clearColor,  1f - (_fadeTime / _t));

            _fadeTime -= Time.deltaTime * _fadeRate;
        }

        //====================================================================================================================//
        public void Init(string text, Vector3 position, Color color)
        {
            Init(text, position, waitTime, fadeTime, floatSpeed, color);
        }
        public void Init(string text, Vector3 position, float waitTime, float fadeTime, float floatSpeed, Color color)
        {
            _t = _fadeTime = fadeTime;
            _fadeRate = 1.0f / fadeTime;
            
            
            _waitTime = waitTime;

            _floatSpeed = floatSpeed;

            _clearColor = _color = color;
            _clearColor.a = 0f;

            TextMeshPro.color = _color;
            TextMeshPro.text = text;

            transform.position = new Vector3(position.x, position.y, 5f);

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
            if (FactoryManager.Instance == null)
                return;
            
            FactoryManager.Instance.GetFactory<ParticleFactory>().CreateObject<FloatingText>()
                .Init(text, position, color);
        }

        //Unity Editor Functionsi
        //====================================================================================================================//

#if UNITY_EDITOR
        [Button, DisableInEditorMode]
        private void Test()
        {
            
            Init("+98765", transform.position, 0.75f,1f,5f, Color.green);
        }
#endif
    }
}
