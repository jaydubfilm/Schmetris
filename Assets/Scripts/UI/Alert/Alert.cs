using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class Alert : Singleton<Alert>
    {
        public static bool Displayed => Instance.windowObject.activeInHierarchy;
        
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
        
        

        //============================================================================================================//
        
        private void Start()
        {
            _positiveButtonText = positiveButton.GetComponentInChildren<TMP_Text>();
            _negativeButtonText = negativeButton.GetComponentInChildren<TMP_Text>();
            _neutralButtonText = neutralButton.GetComponentInChildren<TMP_Text>();

            SetActive(false);

        }
        
        //============================================================================================================//

        public static void ShowAlert(string Title, string Body, string neutralText, Action OnPressedCallback, string dontShowAgainCode = "")
        {
            SetLineHeight(0);
            Instance.Show(Title, Body, neutralText, OnPressedCallback,dontShowAgainCode);
        }

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
            CheckDontShowAgain(dontShowAgainCode);
            SetActive(true);
            
            titleText.text = Title;
            bodyText.text = Body;
            
            positiveButton.gameObject.SetActive(true);
            neutralButton.gameObject.SetActive(false);
            negativeButton.gameObject.SetActive(false);

            _positiveButtonText.text = neutralText;
            positiveButton.onClick.RemoveAllListeners();
            
            positiveButton.onClick.AddListener(() =>
            {
                SetActive(false);
                OnPressedCallback?.Invoke();
            });
        }
        
        private void Show(string Title, string Body, string confirmText, string cancelText, Action<bool> OnConfirmedCallback, string dontShowAgainCode)
        {
            CheckDontShowAgain(dontShowAgainCode);
            SetActive(true);
            
            titleText.text = Title;
            bodyText.text = Body;
            
            positiveButton.gameObject.SetActive(true);
            neutralButton.gameObject.SetActive(false);
            negativeButton.gameObject.SetActive(true);

            _positiveButtonText.text = confirmText;
            positiveButton.onClick.RemoveAllListeners();
            
            positiveButton.onClick.AddListener(() =>
            {
                SetActive(false);
                OnConfirmedCallback?.Invoke(true);
            });
            
            _negativeButtonText.text = cancelText;
            negativeButton.onClick.RemoveAllListeners();
            
            negativeButton.onClick.AddListener(() =>
            {
                SetActive(false);
                OnConfirmedCallback?.Invoke(false);
            });
        }
        
        private void Show(string Title, string Body, string confirmText, string cancelText, string neutralText, Action<bool> OnConfirmedCallback, Action OnNeutralCallback, string dontShowAgainCode)
        {
            CheckDontShowAgain(dontShowAgainCode);
            SetActive(true);
            
            
            titleText.text = Title;
            bodyText.text = Body;
            
            positiveButton.gameObject.SetActive(true);
            neutralButton.gameObject.SetActive(true);
            negativeButton.gameObject.SetActive(true);

            _positiveButtonText.text = confirmText;
            positiveButton.onClick.RemoveAllListeners();
            
            positiveButton.onClick.AddListener(() =>
            {
                SetActive(false);
                OnConfirmedCallback?.Invoke(true);
            });
            
            _negativeButtonText.text = cancelText;
            negativeButton.onClick.RemoveAllListeners();
            
            negativeButton.onClick.AddListener(() =>
            {
                SetActive(false);
                OnConfirmedCallback?.Invoke(false);
            });
            
            _neutralButtonText.text = neutralText;
            neutralButton.onClick.RemoveAllListeners();
            
            neutralButton.onClick.AddListener(() =>
            {
                SetActive(false);
                OnNeutralCallback?.Invoke();
            });
        }
        
        //============================================================================================================//

        public static void SetLineHeight(float lineHeight)
        {
            Instance.bodyText.lineSpacing = lineHeight;
        }

        private void CheckDontShowAgain(string dontShowAgainCode)
        {
            if (string.IsNullOrEmpty(dontShowAgainCode))
            {
                dontShowAgainToggle.gameObject.SetActive(false);
                return;
            }
            
            //TODO Check for the code

            dontShowAgainToggle.gameObject.SetActive(true);
        }
        
        //============================================================================================================//

        private void SetActive(bool state)
        {
            windowObject.SetActive(state);
        }
        
        //============================================================================================================//
    }
}

