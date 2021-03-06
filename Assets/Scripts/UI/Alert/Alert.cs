﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class Alert : Singleton<Alert>, IStartedUsingController, IBuildNavigationProfile
    {
        public static bool Displayed => Instance.windowObject.activeInHierarchy;

        [SerializeField, Required] private TMP_Text versionText;
        
        [SerializeField, Required]
        private GameObject windowObject;

        [SerializeField, Required, BoxGroup("Toggles")]
        private Toggle dontShowAgainToggle;
        
        [SerializeField, Required, BoxGroup("Text")]
        private TMP_Text titleText;
        [SerializeField, Required, BoxGroup("Text")]
        private TMP_Text bodyText;
        
        [SerializeField, Required, BoxGroup("Buttons")]
        private Button positiveButton;
        private TMP_Text _positiveButtonText;
        
        [SerializeField, Required, BoxGroup("Buttons")]
        private Button negativeButton;
        private TMP_Text _negativeButtonText;
        
        [SerializeField, Required, BoxGroup("Buttons")]
        private Button neutralButton;
        private TMP_Text _neutralButtonText;

        private static IReadOnlyList<string> DontShowAgainKeys => PlayerDataManager.GetDontShowAgainKeys();
        
        private string _activeDontShowKey;

        //============================================================================================================//

        private void OnEnable()
        {
            InputManager.AddStartedControllerListener(this);
        }

        private void Start()
        {
            versionText.text = $"v{Application.version}";
            
            _positiveButtonText = positiveButton.GetComponentInChildren<TMP_Text>();
            _negativeButtonText = negativeButton.GetComponentInChildren<TMP_Text>();
            _neutralButtonText = neutralButton.GetComponentInChildren<TMP_Text>();

            SetActive(false);
        }
        
        private void OnDisable()
        {
            InputManager.RemoveControllerListener(this);
        }

        //====================================================================================================================//
        
        /// <summary>
        /// Displays an alert with a single Neutral button. NOTE: using DontShowAgain, if the code has been marked as don't show again, OnPressedCallback will Invoke
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Body"></param>
        /// <param name="neutralText"></param>
        /// <param name="OnPressedCallback"></param>
        /// <param name="dontShowAgainCode"></param>
        public static void ShowAlert(string Title, string Body, string neutralText, Action OnPressedCallback, string dontShowAgainCode = "")
        {
            SetLineHeight(0);
            Instance.Show(Title, Body, neutralText, OnPressedCallback,dontShowAgainCode);
        }

        /// <summary>
        /// Displays an alert with Positive & Negative buttons. NOTE: using DontShowAgain, if the code has been marked as don't show again, OnConfirmedCallback(true) will Invoke
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Body"></param>
        /// <param name="confirmText"></param>
        /// <param name="cancelText"></param>
        /// <param name="OnConfirmedCallback"></param>
        /// <param name="dontShowAgainCode"></param>
        public static void ShowAlert(string Title, string Body, string confirmText, string cancelText,
            Action<bool> OnConfirmedCallback, string dontShowAgainCode = "")
        {
            SetLineHeight(0);
            Instance.Show(Title, Body, confirmText, cancelText, OnConfirmedCallback, dontShowAgainCode);
        }

        public static void ShowAlert(string Title, string Body, string confirmText, string cancelText,
            string neutralText, Action<bool> OnConfirmedCallback, Action OnNeutralCallback, string dontShowAgainCode = "")
        {
            SetLineHeight(0);
            Instance.Show(Title, Body, confirmText, cancelText,neutralText, OnConfirmedCallback, OnNeutralCallback, dontShowAgainCode);
        }
        
        
        //============================================================================================================//

        private void Show(string Title, string Body, string neutralText, Action OnPressedCallback, string dontShowAgainCode)
        {
            var previousInputType = InputManager.CurrentActionMap;
            InputManager.SwitchCurrentActionMap(ACTION_MAP.MENU);
            
            var shouldShowAlert = CheckShouldShowAlert(dontShowAgainCode);
            SetActive(shouldShowAlert);

            if (!shouldShowAlert)
            {
                SetActive(false);
                OnPressedCallback?.Invoke();
                return;
            }
            
            titleText.text = Title;
            bodyText.text = Body;
            
            positiveButton.gameObject.SetActive(true);
            neutralButton.gameObject.SetActive(false);
            negativeButton.gameObject.SetActive(false);

            _positiveButtonText.text = neutralText;
            positiveButton.onClick.RemoveAllListeners();
            
            positiveButton.onClick.AddListener(() =>
            {
                InputManager.SwitchCurrentActionMap(previousInputType);
                CheckDontShowAgain(ref _activeDontShowKey);
                SetActive(false);
                OnPressedCallback?.Invoke();
            });
            
            UISelectHandler.SetBuildTarget(this);
        }
        
        private void Show(string Title, string Body, string confirmText, string cancelText, Action<bool> OnConfirmedCallback, string dontShowAgainCode)
        {
            var previousInputType = InputManager.CurrentActionMap;
            InputManager.SwitchCurrentActionMap(ACTION_MAP.MENU);
            
            var shouldShowAlert = CheckShouldShowAlert(dontShowAgainCode);
            SetActive(shouldShowAlert);

            if (!shouldShowAlert)
            {
                SetActive(false);
                OnConfirmedCallback?.Invoke(true);
                return;
            }
            
            titleText.text = Title;
            bodyText.text = Body;
            
            positiveButton.gameObject.SetActive(true);
            neutralButton.gameObject.SetActive(false);
            negativeButton.gameObject.SetActive(true);

            _positiveButtonText.text = confirmText;
            positiveButton.onClick.RemoveAllListeners();
            
            positiveButton.onClick.AddListener(() =>
            {
                InputManager.SwitchCurrentActionMap(previousInputType);
                CheckDontShowAgain(ref _activeDontShowKey);
                SetActive(false);
                OnConfirmedCallback?.Invoke(true);
            });
            
            _negativeButtonText.text = cancelText;
            negativeButton.onClick.RemoveAllListeners();
            
            negativeButton.onClick.AddListener(() =>
            {
                InputManager.SwitchCurrentActionMap(previousInputType);
                CheckDontShowAgain(ref _activeDontShowKey);
                SetActive(false);
                OnConfirmedCallback?.Invoke(false);
            });
            
            UISelectHandler.SetBuildTarget(this);
        }
        
        private void Show(string Title, string Body, string confirmText, string cancelText, string neutralText, Action<bool> OnConfirmedCallback, Action OnNeutralCallback, string dontShowAgainCode)
        {
            var previousInputType = InputManager.CurrentActionMap;
            InputManager.SwitchCurrentActionMap(ACTION_MAP.MENU);
            
            var shouldShowAlert = CheckShouldShowAlert(dontShowAgainCode);
            SetActive(shouldShowAlert);

            if (!shouldShowAlert)
            {
                SetActive(false);
                OnConfirmedCallback?.Invoke(true);
                return;
            }
            
            
            titleText.text = Title;
            bodyText.text = Body;
            
            positiveButton.gameObject.SetActive(true);
            neutralButton.gameObject.SetActive(true);
            negativeButton.gameObject.SetActive(true);

            _positiveButtonText.text = confirmText;
            positiveButton.onClick.RemoveAllListeners();
            
            positiveButton.onClick.AddListener(() =>
            {
                InputManager.SwitchCurrentActionMap(previousInputType);
                CheckDontShowAgain(ref _activeDontShowKey);
                SetActive(false);
                OnConfirmedCallback?.Invoke(true);
            });
            
            _negativeButtonText.text = cancelText;
            negativeButton.onClick.RemoveAllListeners();
            
            negativeButton.onClick.AddListener(() =>
            {
                InputManager.SwitchCurrentActionMap(previousInputType);
                CheckDontShowAgain(ref _activeDontShowKey);
                SetActive(false);
                OnConfirmedCallback?.Invoke(false);
            });
            
            _neutralButtonText.text = neutralText;
            neutralButton.onClick.RemoveAllListeners();
            
            neutralButton.onClick.AddListener(() =>
            {
                InputManager.SwitchCurrentActionMap(previousInputType);
                CheckDontShowAgain(ref _activeDontShowKey);
                SetActive(false);
                OnNeutralCallback?.Invoke();
            });
            
            UISelectHandler.SetBuildTarget(this);
        }
        
        //============================================================================================================//

        public static void SetLineHeight(float lineHeight)
        {
            Instance.bodyText.lineSpacing = lineHeight;
        }


        //Dont Show Again Functions
        //====================================================================================================================//
        
        /// <summary>
        /// Returns whether or not the Alert should be displayed
        /// </summary>
        /// <param name="dontShowAgainKey"></param>
        /// <returns></returns>
        private bool CheckShouldShowAlert(string dontShowAgainKey)
        {
            if (string.IsNullOrEmpty(dontShowAgainKey))
            {
                dontShowAgainToggle.gameObject.SetActive(false);
                return true;
            }
            
            //Check if the code already exists

            if (DontShowAgainKeys.Contains(dontShowAgainKey))
                return false;

            dontShowAgainToggle.gameObject.SetActive(true);

            _activeDontShowKey = dontShowAgainKey;
            return true;
        }

        private void CheckDontShowAgain(ref string dontShowAgainKey)
        {
            if (string.IsNullOrEmpty(dontShowAgainKey))
                return;

            if (dontShowAgainToggle.isOn)
            {
                PlayerDataManager.AddDontShowAgainKey(dontShowAgainKey);

                dontShowAgainToggle.isOn = false;
                return;
            }
            
            
            dontShowAgainKey = string.Empty;
        }
        
        //============================================================================================================//

        public void SetActive(bool state)
        {
            windowObject.SetActive(state);
        }

        //IStartedUsingController Functions
        //====================================================================================================================//
        
        public void StartedUsingController(bool usingController)
        {
            if (!Displayed) return;

            if (!usingController)
            {
                EventSystem.current?.SetSelectedGameObject(null);
                return;
            }

            if (neutralButton.gameObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(neutralButton.gameObject);
            }
            else if (negativeButton.gameObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(negativeButton.gameObject);
            }
            else if (positiveButton.gameObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(positiveButton.gameObject);
            }
            else
               throw new Exception();
        }
        
        //============================================================================================================//

#if UNITY_EDITOR

        [Button, DisableInEditorMode]
        private void TestAlert()
        {
            throw new Exception("Test Error");
        }
        
#endif

        public NavigationProfile BuildNavigationProfile()
        {

            //--------------------------------------------------------------------------------------------------------//
            
            bool IsSelectable(in Selectable selectable) =>
                selectable.gameObject.activeInHierarchy && selectable.interactable;

            //--------------------------------------------------------------------------------------------------------//
            
            Selectable objectToSelect;
            
            if (IsSelectable(neutralButton))
                objectToSelect = neutralButton;
            else if (IsSelectable(positiveButton))
                objectToSelect = positiveButton;
            else if (IsSelectable(negativeButton))
                objectToSelect = negativeButton;
            else
                throw new Exception();
            
            
            return new NavigationProfile(objectToSelect,
                new []
                {
                    positiveButton,
                    negativeButton,
                    neutralButton
                }, null, null);
        }
    }
}

