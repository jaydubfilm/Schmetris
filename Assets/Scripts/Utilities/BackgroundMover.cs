using System;
using StarSalvager.Cameras.Data;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public class BackgroundMover : MonoBehaviour
    {
        private ORIENTATION _orientation;
        [SerializeField]
        private float backgroundSpeed;
        
        private Material _material;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            _material = GetComponent<Renderer>().material;

            Globals.OrientationChange += SetOrientation;
            SetOrientation(Globals.Orientation);
        }

        // Update is called once per frame
        private void Update()
        {
            SetOffset(Vector2.down * (backgroundSpeed * Time.deltaTime));
        }
        
        //============================================================================================================//

        private void SetOrientation(ORIENTATION newOrientation)
        {
            switch (Globals.Orientation)
            {
                case ORIENTATION.VERTICAL:
                    transform.localRotation = Quaternion.identity;
                    break;
                case ORIENTATION.HORIZONTAL:
                    transform.localRotation = Quaternion.Euler(0,0,270);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newOrientation), newOrientation, null);
            }
        }
        
        //============================================================================================================//

        private void SetOffset(Vector2 offsetDelta)
        {
            var offset = _material.mainTextureOffset;

            offset += offsetDelta;

            if (Mathf.Abs(offset.x) >= 1f)
                offset.x = 0f;
            
            if (Mathf.Abs(offset.y) >= 1f)
                offset.y = 0f;

            _material.mainTextureOffset = offset;
        }
        
        //============================================================================================================//
        
    }
}


