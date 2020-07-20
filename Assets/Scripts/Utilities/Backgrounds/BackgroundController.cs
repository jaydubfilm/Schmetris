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

        private void OnEnable()
        {
            
            backgrounds = GetComponentsInChildren<IBackground>();

            var count = backgrounds.Length;
            
            for (var i = 0; i < count; i++)
            {
                backgrounds[i].Init(cameraTransform, count - i);
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

        public void SetActive(bool state)
        {
            foreach (var background in backgrounds)
            {
                background.gameObject.SetActive(state);
            }   
        }
        
        #if UNITY_EDITOR

        [Button("Disable Backgrounds"), HorizontalGroup("BackgroundEditor")]
        private void DisableBackgrounds()
        {
            SetActive(false);
        }
        
        [Button("Enable Backgrounds"), HorizontalGroup("BackgroundEditor")]
        private void EnableBackgrounds()
        {
            SetActive(true);
        }
        
        #endif

    }

}

