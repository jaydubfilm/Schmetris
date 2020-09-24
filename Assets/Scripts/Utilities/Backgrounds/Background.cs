using System;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.Backgrounds
{
    public class Background : MonoBehaviour, IBackground
    {
        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");
        private static readonly int MainColor = Shader.PropertyToID("_Color");

        #region Properties

        [SerializeField, BoxGroup("Move Values")]
        private Vector2 moveSpeed = Vector2.zero;
        [SerializeField, ReadOnly]
        private Vector2 _moveAmount;
        
        [SerializeField, BoxGroup("Move Values")]
        private float horizontalMoveSpeed;

        public bool parentIsCamera => _parentIsCamera;
        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values")]
        private bool _parentIsCamera;


        public float zDepth => _zDepth;
        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values")]
        private float _zDepth;

        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values")]
        private Vector2 startTiling = Vector2.one;
        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values")]
        private Vector2 startOffset = Vector2.zero;

        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values/Materials"), Required]
        private Material Material;
        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values/Materials"), Required]
        private Texture Texture;
        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values/Materials")]
        private Color color = Color.white;

        #endregion //Properties
        
        //============================================================================================================//
        
        private new Renderer renderer => _renderer ? _renderer : _renderer = GetComponent<Renderer>();
        private Renderer _renderer;

        private Material m_material;
        
        //============================================================================================================//

        public void Init(Transform cameraTransform, float zDepth)
        {
            if (m_material == null)
            {
                m_material = new Material(Material);
                m_material.SetTexture(MainTexture, Texture);
                m_material.SetColor(MainColor, color);
            }
            
            m_material.mainTextureScale = startTiling;
            m_material.mainTextureOffset = startOffset;

            renderer.material = m_material;
            
            _moveAmount = Vector2.zero;
            
            
            _zDepth = zDepth;
            
            var pos = transform.position;
            pos.z = zDepth;
            transform.position = pos;

            if (parentIsCamera)
            {
                transform.SetParent(cameraTransform, true);
            }


            if (moveSpeed != Vector2.zero) 
                return;
            
            enabled = false;
        }
        
        public void UpdatePosition(float moveAmount, bool ignoreInput = false)
        {
            if (moveSpeed == Vector2.zero)
                return;


            var horizontalMove = GameTimer.IsPaused || ignoreInput
                ? Vector2.zero
                : Vector2.right * (horizontalMoveSpeed * moveAmount);
            
            //SetOffset((moveSpeed + horizontalMove) * Time.deltaTime);
            SetOffset((horizontalMove / 75f) + (moveSpeed * Time.deltaTime));
        }
        
        //============================================================================================================//

        private void SetOffset(Vector2 offsetDelta)
        {
            _moveAmount += offsetDelta;
            
            var offset = m_material.mainTextureOffset;

            offset += offsetDelta;
            

            if (Mathf.Abs(_moveAmount.x) >= 1f)
            {
                _moveAmount.x += _moveAmount.x < 0 ? 1f : -1f;
                
                /*if (Mathf.Abs(_moveAmount.x) > 0.1f)
                {
                    System.Console.WriteLine("Test");
                }*/

                offset.x = startOffset.x + _moveAmount.x;
            }

            if (Mathf.Abs(_moveAmount.y) >= 1f)
            {
                _moveAmount.y += _moveAmount.y < 0 ? 1f : -1f;

                /*if (Mathf.Abs(_moveAmount.y) > 0.1f)
                {
                    System.Console.WriteLine("Test");
                }*/

                offset.y = startOffset.y + _moveAmount.y;
            }

            m_material.mainTextureOffset = offset;
        }
        
        public void SetOrientation(ORIENTATION newOrientation)
        {
            if (!parentIsCamera)
                return;
            
            switch (newOrientation)
            {
                case ORIENTATION.VERTICAL:
                    transform.localRotation = Quaternion.identity;
                    break;
                case ORIENTATION.HORIZONTAL:
                    transform.localRotation = Quaternion.Euler(0, 0, 270);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newOrientation), newOrientation, null);
            }
        }

        //====================================================================================================================//
        
    }
}

