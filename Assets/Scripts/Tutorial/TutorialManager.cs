using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;

namespace StarSalvager.Tutorial
{
    public class TutorialManager : MonoBehaviour, IInput
    {
        //sealed 
        private bool _readyForInput;
        private bool _keyPressed;

        //Unity Functions
        //====================================================================================================================//
        
        // Start is called before the first frame update
        void Start()
        {

        }

        //Tutorial Functions
        //====================================================================================================================//
        
        

        //IInput Functions
        //====================================================================================================================//
        
        //TODO Need to setup AnyKey
        
        public void InitInput()
        {
            Input.Actions.Default.Any.Enable();
            Input.Actions.Default.Any.performed += AnyKeyPressed;
        }

        public void DeInitInput()
        {
            Input.Actions.Default.Any.Enable();
            Input.Actions.Default.Any.performed += AnyKeyPressed;
        }

        //Input Functions
        //====================================================================================================================//

        private void AnyKeyPressed(InputAction.CallbackContext ctx)
        {
            _keyPressed = true;
        }

        //Utility Coroutines
        //====================================================================================================================//
        
        private IEnumerator WaitAnyKey()
        {
            yield return new WaitUntil(() => _keyPressed);

            _keyPressed = false;
        }

        private IEnumerator WaitTimeCoroutine(float seconds)
        {
            var t = 0f;

            while (t < seconds)
            {
                t += Time.deltaTime;

                //TODO Update some UI Slider
                
                yield return null;
            }
        }
    }
}
