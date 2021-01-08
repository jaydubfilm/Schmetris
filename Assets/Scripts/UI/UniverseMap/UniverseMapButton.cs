﻿using System;
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

namespace StarSalvager
{
    public enum NodeType
    {
        Level,
        Wreck
    }
    
    [RequireComponent(typeof(Button)), RequireComponent(typeof(PointerEvents))]
    public class UniverseMapButton : MonoBehaviour
    {
        private Action<bool, int, int, RectTransform> _onHoveredCallback;

        [NonSerialized]
        public NodeType NodeType;
        [NonSerialized]
        public int NodeIndex;

        [NonSerialized]
        public Button Button;
        [NonSerialized]
        public PointerEvents PointerEvents;
        public TMP_Text Text;
        public TMP_Text TextBelow;
        [NonSerialized]
        public int SectorNumber = -1;
        [NonSerialized]
        public int WaveNumber = -1;
        public Image BotImage;
        public Image ShortcutImage;
        public Image PointOfInterestImage;

        public void Awake()
        {
            Button = GetComponent<Button>();
            PointerEvents = GetComponent<PointerEvents>();

            BotImage.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            //TODO Need to get the level here
            BotImage.sprite = FactoryManager.Instance.PartsProfileData.GetProfile(PART_TYPE.CORE).GetSprite(0);
        }

        public void Start()
        {
            Button.onClick.AddListener(() =>
            {
                switch(NodeType)
                {
                    case NodeType.Level:
                        Globals.CurrentSector = SectorNumber;
                        Globals.CurrentWave = WaveNumber;

                        ScreenFade.Fade(() =>
                        {
                            SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.UNIVERSE_MAP);
                        });
                        break;
                    case NodeType.Wreck:
                        PlayerDataManager.SetCurrentNode(NodeIndex);

                        if (!PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Contains(NodeIndex))
                        {
                            PlayerDataManager.AddCompletedNode(NodeIndex);
                        }

                        ScreenFade.Fade(() =>
                        {
                            SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.UNIVERSE_MAP);
                        });
                        break;
                }
            });
        }

        public void Update()
        {
            PulseBotObject();
        }

        public void PulseBotObject()
        {
            if (!BotImage.gameObject.activeSelf)
            {
                return;
            }

            float scale = 1.0f + Mathf.PingPong(Time.time / 3, 0.25f);
            BotImage.gameObject.transform.localScale = Vector3.one * scale;
        }

        public void SetupHoveredCallback(Action<bool, int, int, RectTransform> onHoveredCallback)
        {
            _onHoveredCallback = onHoveredCallback;

            PointerEvents.PointerEntered += hovered =>
            {
                if (hovered)
                    _onHoveredCallback?.Invoke(true, SectorNumber, WaveNumber, gameObject.transform as RectTransform);
                else
                    _onHoveredCallback?.Invoke(false, -1, -1, null);
            };
        }
    }
}