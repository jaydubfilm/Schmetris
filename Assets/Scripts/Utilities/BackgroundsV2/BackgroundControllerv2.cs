using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.Backgrounds
{
    public class BackgroundControllerv2 : MonoBehaviour, IPausable
    {
        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");
        private static readonly int MainColor = Shader.PropertyToID("_Color");
        
        public enum TYPE
        {
            STATIC,
            SPRITE,
            TRANSPARENT,
            CUTOUT
        }

        [Serializable]
        public class Background
        {
            [FoldoutGroup("$GetName")]
            public TYPE type;

            [FoldoutGroup("$GetName")]
            public Transform Transform;
            [HideInInspector]
            public Renderer Renderer;

            [HideInInspector]
            public float yScale;
            [HideInInspector]
            public float xScale;

            [HideInInspector]
            public Material MaterialInstance;

            [FoldoutGroup("$GetName")]
            public Vector2 startOffset = Vector2.zero;
            [FoldoutGroup("$GetName")]
            public Vector2 startTile = Vector2.one;

            [FoldoutGroup("$GetName")]
            public Texture Texture;
            [FoldoutGroup("$GetName")]
            public Color Color = Color.white;

            [FoldoutGroup("$GetName")]
            public bool isInfinite;
            [FoldoutGroup("$GetName"), DisableIf("isInfinite")]
            public float distance;

#if UNITY_EDITOR
            private string GetName()
            {
                return Transform ? Transform.gameObject.name : string.Empty;
            }
#endif
        }

        [SerializeField]
        private Material transparentMaterial;
        [SerializeField]
        private Material cutoutMaterial;
        
        [SerializeField]
        private Camera _camera;

        [SerializeField, Range(1f, 100f)] 
        private float globalSpeedOffset = 10f;

        [SerializeField] 
        private bool IgnorePause;

        [SerializeField]
        private Background[] backgrounds;

        private new Transform transform;

        private Vector3 _delta;
        private Vector3 _currentPos, _lastPos;

        //IPausable Properties
        //====================================================================================================================//
        
        public bool isPaused { get; private set; }

        //====================================================================================================================//

        private void Awake()
        {
            transform = gameObject.transform;
            
            transform.SetParent(_camera.transform, true);

            _currentPos = _lastPos = transform.position;

        }

        private void OnEnable()
        {
            SetupBackgrounds();
        }

        // Start is called before the first frame update
        private void Start()
        {
            RegisterPausable();
        }

        // Update is called once per frame
        private void Update()
        {
            if (isPaused)
                return;

            CalculateCameraDelta();
            
            //TODO Need to get the current movement delta
            MoveBackgrounds();
        }

        //====================================================================================================================//

        private void SetupBackgrounds()
        {
            for (var i = 0; i < backgrounds.Length; i++)
            {
                var background = backgrounds[i];

                var startPosition = background.Transform.localPosition;
                
                startPosition.z = background.isInfinite
                    ? _camera.farClipPlane + _camera.transform.localPosition.z
                    : background.distance - _camera.transform.localPosition.z;

                background.Transform.localPosition = startPosition;
                
                switch (background.type)
                {
                    case TYPE.STATIC:
                        continue;
                    case TYPE.SPRITE:
                        backgrounds[i].Renderer = backgrounds[i].Transform.GetComponent<SpriteRenderer>();
                        break;
                    case TYPE.TRANSPARENT:
                        SetMaterialInstance(transparentMaterial, ref backgrounds[i]);
                        break;
                    case TYPE.CUTOUT:
                        SetMaterialInstance(cutoutMaterial, ref backgrounds[i]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void MoveBackgrounds()
        {
            Vector2 offset = Vector2.zero;

            foreach (var background in backgrounds)
            {
                if (background.isInfinite)
                    continue;

                switch (background.type)
                {
                    //------------------------------------------------------------------------------------------------//

                    case TYPE.STATIC:
                        continue;
                    
                    //------------------------------------------------------------------------------------------------//

                    case TYPE.SPRITE:
                        //TODO Need to check and see if the image should wrap
                        CheckSpriteWrap(background);
                        
                        var xPosDelta = -_delta.x / (background.distance / globalSpeedOffset);
                        
                        var yPosDelta = Globals.TimeForAsteroidToFallOneSquare / (background.distance / globalSpeedOffset);
                        
                        background.Transform.localPosition += new Vector3(xPosDelta, -yPosDelta) * Time.deltaTime;
                        break;
                    
                    //------------------------------------------------------------------------------------------------//
                    
                    case TYPE.TRANSPARENT:
                    case TYPE.CUTOUT:
                        var xDelta = (1f / background.xScale) *
                                     (_delta.x / (background.distance / globalSpeedOffset)) *
                                     background.startTile.x;
                        
                        var yDelta = (1f / background.yScale) *
                                     (Globals.TimeForAsteroidToFallOneSquare / (background.distance / globalSpeedOffset)) *
                                     background.startTile.y;
                        
                        //offset = Vector2.up * (yDelta * Time.deltaTime);
                        offset = new Vector2(xDelta, yDelta) * Time.deltaTime;

                        //FIXME This offset doesn't work correctly in build. Unsure if the camera delta or something else is causing the issue
                        background.MaterialInstance.mainTextureOffset += offset;
                        break;
                    //------------------------------------------------------------------------------------------------//

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        //Delta Functions
        //====================================================================================================================//

        private void CalculateCameraDelta()
        {
            _lastPos = _currentPos;
            _currentPos = transform.position;

            
            _delta = _currentPos - _lastPos;
        }

        private void CheckSpriteWrap(in Background background)
        {
            var pos = background.Transform.localPosition;
            var spriteRenderer = (SpriteRenderer)background.Renderer;
            
            var lowestPoint = _camera.ViewportToWorldPoint(Vector3.zero).y;
            var highestPoint = _camera.ViewportToWorldPoint(Vector3.one).y;
            
            if (pos.y < lowestPoint - spriteRenderer.bounds.extents.y)
            {
                pos.y = Mathf.Abs(highestPoint + spriteRenderer.bounds.extents.y * 1.2f);
                
                background.Transform.localPosition = pos;
            }
        }

        //IPausable Functions
        //====================================================================================================================//

        public void RegisterPausable()
        {
            if (!IgnorePause)
                GameTimer.AddPausable(this);
        }

        public void OnResume()
        {
            isPaused = false;
        }

        public void OnPause()
        {
            isPaused = true;
        }

        //====================================================================================================================//

        private static void SetMaterialInstance(Material material, ref Background background)
        {
            background.yScale = background.Transform.localScale.y;
            background.xScale = background.Transform.localScale.x;

            if (background.MaterialInstance == null)
            {
                background.MaterialInstance = new Material(material);
                background.MaterialInstance.SetTexture(MainTexture, background.Texture);
                background.MaterialInstance.SetColor(MainColor, background.Color);
            }

            background.MaterialInstance.mainTextureOffset = background.startOffset;
            background.MaterialInstance.mainTextureScale = background.startTile;

            background.Renderer = background.Transform.GetComponent<Renderer>();
            background.Renderer.material = background.MaterialInstance;
        }

        //====================================================================================================================//



    }
}
