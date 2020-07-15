using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.Backgrounds
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField, Required, DisableInPlayMode]
        private Transform cameraTransform;
        
        [SerializeField, ReadOnly]
        private IBackground[] backgrounds;
        
        //================================================================================================================//

        private void Start()
        {
            backgrounds = GetComponentsInChildren<IBackground>();
            
            foreach (var background in backgrounds)
            {
                background.Init(cameraTransform);
            }

            Globals.OrientationChange += SetOrientation;
            SetOrientation(Globals.Orientation);
        }
        private void LateUpdate()
        {
            foreach (var background in backgrounds)
            {
                background.UpdatePosition();
            }
        }

        //================================================================================================================//

        private void SetOrientation(ORIENTATION newOrientation)
        {
            foreach (var background in backgrounds)
            {
                background.SetOrientation(newOrientation);
            }
            
        }

        //================================================================================================================//

    }

}

