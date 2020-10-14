using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Input = StarSalvager.Utilities.Inputs.Input;

namespace StarSalvager.Tutorial
{
    public class TutorialManager : MonoBehaviour, IInput
    {
        [Serializable]
        public struct TutorialStepData
        {
            [FoldoutGroup("$title")]
            public string title;
            [HorizontalGroup("$title/UseWait"), ToggleLeft, LabelWidth(50f)]
            public bool useWaitTime;
            [HorizontalGroup("$title/UseWait"), EnableIf("useWaitTime"), HideLabel, SuffixLabel("Seconds", true)]
            public float waitTime;
            [TextArea,FoldoutGroup("$title")]
            public string text;
        }

        [SerializeField]
        private List<TutorialStepData> tutorialStepDatas;
        

        [SerializeField]
        private TMP_Text text;
        [SerializeField]
        private Slider slider;
        [SerializeField]
        private Image image;
        
        private bool _readyForInput;
        private bool _keyPressed;
p
        private List<IEnumerator> _tutorialStepCoroutines;

        //Unity Functions
        //====================================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            InitInput();

            SetupTutorial();
        }

        private void OnDisable()
        {
            DeInitInput();
        }

        //Tutorial Functions
        //====================================================================================================================//

        private void SetupTutorial()
        {
            _tutorialStepCoroutines = new List<IEnumerator>
            {
                //Step1Coroutine(),
                //Step2Coroutine(),
                //Step3Coroutine(),
            };
            
            StartCoroutine(MainTutorialCoroutine());

        }

        private IEnumerator MainTutorialCoroutine()
        {
            foreach (var stepCoroutine in _tutorialStepCoroutines)
            {
                yield return StartCoroutine(stepCoroutine);
            }
        }

        //Generic Tutorial Steps
        //====================================================================================================================//

        private IEnumerator WaitAnyKeyStep(TutorialStepData tutorialStepData)
        {
            text.text = tutorialStepData.text;
            yield return new WaitForAnyKey(this);
        }
        private IEnumerator WaitTimeSliderStep(TutorialStepData tutorialStepData)
        {
            text.text = tutorialStepData.text;
            
            yield return new WaitTimeSlider(tutorialStepData.waitTime, slider);
            
            yield return new WaitForAnyKey(this);
        }
        
        private IEnumerator WaitTimeImageStep(TutorialStepData tutorialStepData)
        {
            text.text = tutorialStepData.text;
            
            yield return new WaitTimeImage(tutorialStepData.waitTime, image);
            
            yield return new WaitForAnyKey(this);
        }

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
            Input.Actions.Default.Any.Disable();
            Input.Actions.Default.Any.performed -= AnyKeyPressed;
        }

        //Input Functions
        //====================================================================================================================//

        private void AnyKeyPressed(InputAction.CallbackContext ctx)
        {
            var pressed = ctx.ReadValue<float>() == 1f;
            
            _keyPressed = pressed;
        }

        //Utility Coroutines
        //====================================================================================================================//
        
        private sealed class WaitForAnyKey : IEnumerator
        {
            private readonly TutorialManager _tutorialManager;
            
            public bool MoveNext()
            {
                if (!_tutorialManager._keyPressed) 
                    return true;
                
                _tutorialManager._keyPressed = false;
                Debug.Log("Any Key Pressed");
                return false;
            }

            public void Reset() { }

            public object Current => null;

            public WaitForAnyKey(TutorialManager tutorialManager)
            {
                Debug.Log("Waiting for Any Key");
                _tutorialManager = tutorialManager;
            }
        }
        private sealed class WaitTimeSlider : IEnumerator
        {
            private readonly Slider _slider;
            private readonly float _startTime;
            private float _timePast;
            
            public bool MoveNext()
            {
                if (_timePast >= _startTime)
                    return false;
                
                _timePast += Time.deltaTime;
                _slider.value = _timePast / _startTime;
                
                return true;
            }

            public void Reset() { }

            public object Current => null;

            public WaitTimeSlider(float time, Slider slider)
            {
                _startTime = time;
                _timePast = 0;
                _slider = slider;
            }
        }
        private sealed class WaitTimeImage : IEnumerator
        {
            private readonly Image _image;
            private readonly float _startTime;
            private float _timePast;
            
            public bool MoveNext()
            {
                if (_timePast >= _startTime)
                    return false;
                
                _timePast += Time.deltaTime;
                _image.fillAmount = _timePast / _startTime;
                
                return true;
            }

            public void Reset() { }

            public object Current => null;

            public WaitTimeImage(float time, Image image)
            {
                _startTime = time;
                _timePast = 0;
                _image = image;
            }
        }
        //====================================================================================================================//
        
    }
}
