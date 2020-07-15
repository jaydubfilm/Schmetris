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
        
        [SerializeField, BoxGroup("Move Values")]
        private Vector2 moveSpeed = Vector2.zero;
        private Vector2 _moveAmount;
        
        [SerializeField, BoxGroup("Move Values")]
        private float horizontalMoveSpeed;

        public bool parentIsCamera => _parentIsCamera;
        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values")]
        private bool _parentIsCamera;


        public bool ignoreOrientationChanges => _ignoreOrientationChanges;
        [SerializeField, DisableInPlayMode, BoxGroup("Starting Values")]
        private bool _ignoreOrientationChanges;


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

        //============================================================================================================//
        
        private Material m_material;
        private new Renderer renderer;

        //private float dasTimer;
        
        //============================================================================================================//

        public void Init(Transform cameraTransform)
        {
            renderer = GetComponent<Renderer>();
            
            m_material = new Material(Material);
            m_material.SetTexture(MainTexture, Texture);
            m_material.SetColor(MainColor, color);
            m_material.mainTextureScale = startTiling;
            m_material.mainTextureOffset = startOffset;

            renderer.material = m_material;
            
            var pos = transform.position;
            pos.z = zDepth;
            transform.position = pos;

            if (parentIsCamera)
            {
                transform.SetParent(cameraTransform, true);
            }


            if (moveSpeed == Vector2.zero)
            {
                enabled = false;
                return;
            }

            //RegisterMoveOnInput();
        }
        
        public void UpdatePosition()
        {
            if (moveSpeed == Vector2.zero)
                return;

            //if (dasTimer > 0f)
            //    dasTimer -= Time.deltaTime;
            //else
            //{
            //    _horizontalDirecion = _pendingHorizontalDirecion;
            //}

            var horizontalMove = Vector2.right * (horizontalMoveSpeed * Globals.MovingDirection.GetHorizontalDirectionFloat());
            
            
            SetOffset((moveSpeed + horizontalMove) * Time.deltaTime);
        }
        
        //============================================================================================================//

        private void SetOffset(Vector2 offsetDelta)
        {
            _moveAmount += offsetDelta;
            
            var offset = m_material.mainTextureOffset;

            offset += offsetDelta;

            if (Mathf.Abs(_moveAmount.x) >= 1f)
            {
                _moveAmount.x = 0f;
                offset.x = startOffset.x;
            }

            if (Mathf.Abs(_moveAmount.y) >= 1f)
            {
                _moveAmount.y = 0f;
                offset.y =startOffset.y;
            }

            m_material.mainTextureOffset = offset;
        }
        
        public void SetOrientation(ORIENTATION newOrientation)
        {
            if (ignoreOrientationChanges)
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
        
        //IMoveOnInput
        //============================================================================================================//

        /*public void RegisterMoveOnInput()
        {
            InputManager.RegisterMoveOnInput(this);
        }*/

        /*
        //private float _horizontalDirecion;
        //private float _pendingHorizontalDirecion;

        public void Move(float direction)
        {
            //if (Globals.MovingDirection)
            //{
            //    _pendingHorizontalDirecion = direction;
            //    return;
            //}
            //
            //_pendingHorizontalDirecion = _horizontalDirecion = direction;
//
            ////dasTimer = Globals.DASTime;
        }*/
    }
}

