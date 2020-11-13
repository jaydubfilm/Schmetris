using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace StarSalvager.Utilities.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextUnderlay : MonoBehaviour
    {
        private TMP_Text _mainText;

        [SerializeField, OnValueChanged("UpdateUnderlayElement")]
        private Color underlayColor = Color.white;
        [SerializeField, Range(0f, 1f), OnValueChanged("UpdateUnderlayElement")] 
        private float underlayOffset = 0.06f;

        private string _currentText;
        private float _currentFontSize;
        private RectTransform _targetTransform;
        private TMP_Text _targetText;

        private bool _ready;

        //Unity Functions
        //====================================================================================================================//


        // Start is called before the first frame update
        private void Start()
        {
            _mainText = GetComponent<TMP_Text>();
            //TODO Create the new element below the original
            //TODO Set the anchors and position offset
            CreateUnderlayAsset();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_ready)
                return;
            
            //TODO Check for changes in text size, then adjust accordingly

            if (System.Math.Abs(_mainText.fontSize - _currentFontSize) > 0.01f || !_mainText.text.Equals(_currentText))
                UpdateUnderlayElement();
        }

        //TextUnderlay Functions
        //====================================================================================================================//

        private void CreateUnderlayAsset()
        {
            _targetText = Instantiate(_mainText, _mainText.transform, false);
            Destroy(_targetText.GetComponent<TextUnderlay>());
            
            _targetText.color = _mainText.color;

            _targetTransform = (RectTransform)_targetText.transform;
            _targetTransform.sizeDelta = Vector2.zero;
            
            _targetTransform.anchorMin = Vector2.zero;
            _targetTransform.anchorMax = Vector2.one;
            
            UpdateUnderlayElement();
            _ready = true;
        }

        private void UpdateUnderlayElement()
        {
            if (!_targetText)
                return;
            
            _targetText.text = _mainText.text;
            _targetText.fontSize = _mainText.fontSize;

            _mainText.color = underlayColor;
            
            _targetTransform.anchoredPosition = Vector2.up * (_mainText.fontSize * underlayOffset);

            _currentText = _mainText.text;
            _currentFontSize = _mainText.fontSize;
        }


        //====================================================================================================================//
        
    }

}