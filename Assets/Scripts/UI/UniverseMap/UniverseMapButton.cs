using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager
{
    public class UniverseMapButton : MonoBehaviour
    {
        public Button Button;
        public TMP_Text Text;
        public int SectorNumber;
        public bool ButtonsActive = false;

        [SerializeField, Required]
        private UniverseWaveButton m_waveButtonPrefab;

        private List<UniverseWaveButton> m_waveButtons;

        public void SetupWaveButtons(int numberWaves)
        {
            m_waveButtons = new List<UniverseWaveButton>();

            for (int i = 0; i < numberWaves; i++)
            {
                UniverseWaveButton button = GameObject.Instantiate(m_waveButtonPrefab);
                button.transform.SetParent(transform);
                m_waveButtons.Add(button);
                button.WaveNumber = i;
                button.Text.text = "Wave " + (i + 1);
                button.Button.onClick.AddListener(() =>
                {
                    SetActiveWaveButtons(false);
                    Values.Globals.CurrentSector = SectorNumber;
                    Values.Globals.CurrentWave = button.WaveNumber;
                    AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, eventDataParameter: Values.Globals.CurrentSector);
                    SceneLoader.ActivateScene(SceneLoader.ALEX_TEST_SCENE, SceneLoader.UNIVERSE_MAP);
                });
                button.transform.position = new Vector2
                    (transform.position.x + 80 * Mathf.Cos((((float)i / (float)numberWaves) * 360 - 90) * -1 * Mathf.Deg2Rad), 
                    transform.position.y + 80 * Mathf.Sin((((float)i / (float)numberWaves) * 360 - 90) * -1 * Mathf.Deg2Rad));
            }
            SetActiveWaveButtons(false);

            //position buttons
        }

        public void SetActiveWaveButtons(bool active)
        {
            foreach (var button in m_waveButtons)
            {
                button.gameObject.SetActive(active);
                button.Button.interactable = PlayerPersistentData.PlayerData.CheckIfQualifies(SectorNumber, button.WaveNumber);
            }
            ButtonsActive = active;
        }
    }
}
