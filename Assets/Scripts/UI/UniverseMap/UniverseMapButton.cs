using System;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager
{
    [RequireComponent(typeof(Button)), RequireComponent(typeof(PointerEvents))]
    public class UniverseMapButton : MonoBehaviour
    {
        private Action<bool, int, int, RectTransform> _onHoveredCallback;

        [NonSerialized]
        public Button Button;
        [NonSerialized]
        public PointerEvents PointerEvents;
        public TMP_Text Text;
        [NonSerialized]
        public int SectorNumber = -1;
        [NonSerialized]
        public int WaveNumber = -1;

        public void Awake()
        {
            Button = GetComponent<Button>();
            PointerEvents = GetComponent<PointerEvents>();
        }

        public void Start()
        {
            Button.onClick.AddListener(() =>
            {
                if (SectorNumber < 0 || WaveNumber < 0)
                {
                    return;
                }
                
                Globals.CurrentSector = SectorNumber;
                Globals.CurrentWave = WaveNumber;
                SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.UNIVERSE_MAP);
            });
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