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
        
        public NodeType NodeType => nodeType;
        public int NodeIndex => nodeIndex;
        
        [SerializeField, ReadOnly]
        private NodeType nodeType;

        [SerializeField, ReadOnly] private int nodeIndex;

        [SerializeField] private Button Button;
        [SerializeField] private TMP_Text Text;
        [SerializeField] private TMP_Text TextBelow;

        [SerializeField] private int ringIndex = -1;
        [SerializeField] private int waveIndex = -1;
        
        [SerializeField] private Image BotImage;
        [SerializeField] private Image ShortcutImage;
        //[SerializeField] private Image PointOfInterestImage;

        public new RectTransform transform { get; private set; }

        //Unity Functions
        //====================================================================================================================//

        public void Awake()
        {
            transform = gameObject.transform as RectTransform;
        }

        public void Start()
        {
            Button.onClick.AddListener(() =>
            {
                switch (nodeType)
                {
                    case NodeType.Base:
                        PlayerDataManager.SetCurrentNode(PlayerDataManager.GetCurrentNode() + 1);
                        ScreenFade.Fade(() =>
                        {
                            _universeMap.ForceDrawMap();
                        });
                        break;
                    case NodeType.Level:
                        Globals.CurrentRingIndex = ringIndex;
                        Globals.CurrentWave = waveIndex;

                        ScreenFade.Fade(() =>
                        {
                            SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.UNIVERSE_MAP);
                        });
                        break;
                    case NodeType.Wreck:
                        PlayerDataManager.SetCurrentNode(nodeIndex);

                        if (!PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Contains(nodeIndex))
                        {
                            PlayerDataManager.AddCompletedNode(nodeIndex);
                        }

                        ScreenFade.Fade(() =>
                        {
                            SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.UNIVERSE_MAP, MUSIC.SCRAPYARD);
                        });
                        break;
                }
            });
        }

        public void Update()
        {
            PulseBotObject();
        }

        //====================================================================================================================//

        public void Init(in int nodeIndex, in int ringIndex, in NodeData nodeData)
        {

            if (!_universeMap)
                _universeMap = FindObjectOfType<UniverseMap>();
            
            if (!transform)
                transform = gameObject.transform as RectTransform;
            
            BotImage.sprite = FactoryManager.Instance.PartsProfileData.GetProfile(PART_TYPE.EMPTY).GetSprite(0);

            this.nodeIndex = nodeIndex;

            SetWaveIndex(ringIndex, nodeData.waveIndex);

            nodeType = nodeData.NodeType;


            SetShortcutImageActive(NodeType == NodeType.Wreck);
            
            string title;
            switch (NodeType)
            {
                case NodeType.Base:
                    title = "Base";
                    break;
                case NodeType.Level:
                    title = $"{ringIndex + 1}.{waveIndex + 1}";
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

        public void SetWaveIndex(in int ringIndex, in int waveIndex)
        {
            this.ringIndex = ringIndex;
            this.waveIndex = waveIndex;
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