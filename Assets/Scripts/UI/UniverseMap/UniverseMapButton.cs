using System;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Audio;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager.Utilities.Saving;
using System.Linq;
using StarSalvager.AI;
using StarSalvager.UI;
using StarSalvager.Utilities.Interfaces;
using UnityEngine.Serialization;

namespace StarSalvager
{
    public enum NodeType
    {
        Base,
        Level,
        Wreck
    }

    [RequireComponent(typeof(Button)), RequireComponent(typeof(PointerEvents))]
    public class UniverseMapButton : MonoBehaviour
    {
        private static UniverseMap _universeMap;

        public bool IsButtonInteractable => Button.interactable;
        
        public NodeType NodeType => nodeType;
        public int NodeIndex => nodeIndex;
        
        [SerializeField, ReadOnly]
        private NodeType nodeType;

        [SerializeField, ReadOnly] private int nodeIndex;

        [SerializeField] private Button Button;
        [SerializeField] private TMP_Text Text;
        [SerializeField] private TMP_Text TextBelow;

        [SerializeField] private Image BotImage;
        [SerializeField] private Image ShortcutImage;

        //[SerializeField] private Image PointOfInterestImage;

        public new RectTransform transform
        {
            get
            {
                if(_transform == null)
                    _transform = gameObject.transform as RectTransform;

                return _transform;
            }
        }
        private RectTransform _transform;

        //====================================================================================================================//

        public void Init(in int nodeIndex, in int sector, in NodeType nodeType, Action<int, NodeType> onPressedCallback)
        {
            if (!_universeMap) _universeMap = FindObjectOfType<UniverseMap>();
            
            Button.onClick.RemoveAllListeners();
            
            BotImage.sprite = PART_TYPE.EMPTY.GetSprite();

            this.nodeIndex = nodeIndex;
            this.nodeType = nodeType;
            
            Button.onClick.AddListener(() => onPressedCallback?.Invoke(this.nodeIndex, this.nodeType));


            SetShortcutImageActive(NodeType == NodeType.Wreck);
            
            string title;
            switch (NodeType)
            {
                case NodeType.Base:
                    title = "Base";
                    break;
                case NodeType.Level:
                    title = $"{nodeIndex}";
                    break;
                case NodeType.Wreck:
                    title = "Wreck";
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            SetTitles(title, string.Empty);

        }

        public void SetTitles(in string title, in string subTitle)
        {
            Text.text = title;
            TextBelow.text = subTitle;
        }

        public void SetButtonProperties(in bool buttonInteractable, in Color color)
        {
            SetButtonColor(color);
            SetButtonInteractable(buttonInteractable);
        }
        public void SetButtonColor(in Color buttonColor)
        {
            Button.image.color = buttonColor;
        }
        public void SetButtonInteractable(in bool buttonInteractable)
        {
            Button.interactable = buttonInteractable;
        }

        public void SetBotImageActive(in bool state)
        {
            BotImage.gameObject.SetActive(state);
        }

        public void SetShortcutImageActive(in bool state)
        {
            ShortcutImage.gameObject.SetActive(state);
        }

        /*public void SetWaveType(in NodeType nodeType, in int waveIndex)
        {
            this.nodeType = nodeType;
            switch (nodeType)
            {
                case NodeType.Level when waveIndex < 0:
                    throw new ArgumentException("Missing the wave number for level");
                case NodeType.Level:
                    waveNumber = waveIndex;
                    break;
            }
        }*/

        public void Reset()
        {
            SetTitles(string.Empty, string.Empty);
            SetButtonProperties(false, Color.white);
            SetBotImageActive(false);
            SetShortcutImageActive(false);
        }


        private void PulseBotObject()
        {
            if (!BotImage.gameObject.activeSelf)
            {
                return;
            }

            float scale = 1.0f + Mathf.PingPong(Time.time / 3, 0.25f);
            BotImage.gameObject.transform.localScale = Vector3.one * scale;
        }

        /*public void SetupHoveredCallback(Action<bool, int, int, RectTransform> onHoveredCallback)
        {
            _onHoveredCallback = onHoveredCallback;

            PointerEvents.PointerEntered += hovered =>
            {
                if (hovered)
                    _onHoveredCallback?.Invoke(true, SectorNumber, WaveNumber, gameObject.transform as RectTransform);
                else
                    _onHoveredCallback?.Invoke(false, -1, -1, null);
            };
        }*/

        //====================================================================================================================//

    }
}