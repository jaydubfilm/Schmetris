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
    public class UniverseMapButton : MonoBehaviour
    {
        private Action<bool, int, int, RectTransform> _onHoveredCallback;
        
        public Button Button;
        public TMP_Text Text;
        public int SectorNumber;
        public bool ButtonsActive = false;

        [SerializeField, Required]
        private UniverseWaveButton m_waveButtonPrefab;

        private List<UniverseWaveButton> m_waveButtons;

        public void SetupWaveButtons(int numberWaves, Action<bool, int, int, RectTransform> onHoveredCallback)
        {
            _onHoveredCallback = onHoveredCallback;
            
            m_waveButtons = new List<UniverseWaveButton>();

            for (int i = 0; i < numberWaves; i++)
            {
                var waveNumber = i;
                var waveButton = Instantiate(m_waveButtonPrefab, transform, true);
                m_waveButtons.Add(waveButton);
                waveButton.WaveNumber = i;
                waveButton.Text.text = $"Wave {waveNumber + 1}";
                waveButton.Button.onClick.AddListener(() =>
                {
                    SetActiveWaveButtons(false);
                    Globals.CurrentSector = SectorNumber;
                    Globals.CurrentWave = waveButton.WaveNumber;
                    //AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, eventDataParameter: Values.Globals.CurrentSector);
                    SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.UNIVERSE_MAP);
                });
                waveButton.transform.position = new Vector2
                    (transform.position.x + 80 * Mathf.Cos((((float)i / (float)numberWaves) * 360 - 90) * -1 * Mathf.Deg2Rad), 
                    transform.position.y + 80 * Mathf.Sin((((float)i / (float)numberWaves) * 360 - 90) * -1 * Mathf.Deg2Rad));

                waveButton.GetComponent<PointerEvents>().PointerEntered += hovered =>
                {
                    if(hovered)
                        _onHoveredCallback?.Invoke(true, SectorNumber, waveNumber, waveButton.transform as RectTransform);
                    else
                        _onHoveredCallback?.Invoke(false, -1, -1, null);
                };
            }
            SetActiveWaveButtons(false);

            //position buttons
        }

        public void SetActiveWaveButtons(bool active)
        {
            foreach (var button in m_waveButtons)
            {
                button.gameObject.SetActive(active);
                if (!Globals.DisableTestingFeatures)
                {
                    button.Button.interactable = true;
                }
                else if (!Globals.AllowAccessToUnlockedLaterWaves)
                {
                    button.Button.interactable = button.WaveNumber == 0 && PlayerPersistentData.PlayerData.CheckIfQualifies(SectorNumber, button.WaveNumber);
                }
                else
                {
                    button.Button.interactable = PlayerPersistentData.PlayerData.CheckIfQualifies(SectorNumber, button.WaveNumber);
                }
            }
            ButtonsActive = active;
        }
        
    }
}
