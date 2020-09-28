using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using System.Collections.Generic;
using StarSalvager.Utilities.Math;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class UniverseMap : MonoBehaviour, IReset
    {
        [SerializeField, Required] private UniverseMapButton m_universeSectorButtonPrefab;

        [SerializeField, Required] private ScrollRect m_scrollRect;
        [SerializeField, Required] private RectTransform m_scrollRectArea;

        private List<UniverseMapButton> currentUniverseButtons = new List<UniverseMapButton>();

        [SerializeField, Required]
        private Button swapUniverseButton;
        [SerializeField, Required]
        private Button backButton;

        //============================================================================================================//

        private void Start()
        {
            InitButtons();
        }

        public void Activate()
        {
            InitUniverseMapTemp();
        }

        public void Reset()
        {
            
        }

        //============================================================================================================//

        private void InitButtons()
        {
            swapUniverseButton.onClick.AddListener(() =>
            {
                if (FactoryManager.Instance.currentModularDataIndex == FactoryManager.Instance.ModularDataCount - 1)
                {
                    FactoryManager.Instance.currentModularDataIndex = 0;
                }
                else
                {
                    FactoryManager.Instance.currentModularDataIndex++;
                }
                PlayerPersistentData.PlayerData.currentModularSectorIndex = FactoryManager.Instance.currentModularDataIndex;

                InitUniverseMapTemp();
            });
            
            backButton.onClick.AddListener(() => SceneLoader.LoadPreviousScene());
        }

        //============================================================================================================//

        private void InitUniverseMap()
        {
            foreach (var button in currentUniverseButtons)
            {
                GameObject.Destroy(button.gameObject);
            }
            currentUniverseButtons.Clear();

            HaltonSequence positionSequence = new HaltonSequence();

            Rect rect = m_scrollRectArea.rect;

            Vector2 size = new Vector2(rect.xMax - rect.xMin, rect.yMax - rect.yMin);
            positionSequence.Reset();

            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
                if (Globals.DisableTestingFeatures && !PlayerPersistentData.PlayerData.CheckIfQualifies(i, 0))
                {
                    continue;
                }
                
                positionSequence.Increment();

                var position = positionSequence.m_CurrentPos;

                position.x -= 0.5f;
                position.y -= 0.5f;
                position.z = 0.0f;
                position.x *= size.x;
                position.y *= size.y;

                UniverseMapButton button = Instantiate(m_universeSectorButtonPrefab);
                button.SetupWaveButtons(FactoryManager.Instance.SectorRemoteData[i].GetNumberOfWaves());
                button.transform.SetParent(m_scrollRectArea.transform);
                button.transform.localPosition = position;
                button.Text.text = $"Sector {i + 1}";
                button.SectorNumber = i;
                button.Button.onClick.AddListener(() => { button.SetActiveWaveButtons(!button.ButtonsActive); });
                currentUniverseButtons.Add(button);
            }
        }

        private void InitUniverseMapTemp()
        {
            foreach (var button in currentUniverseButtons)
            {
                GameObject.Destroy(button.gameObject);
            }
            currentUniverseButtons.Clear();

            Rect rect = m_scrollRectArea.rect;
            RectTransform centerOn = null;
            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
                if (Globals.DisableTestingFeatures && !PlayerPersistentData.PlayerData.CheckIfQualifies(i, 0))
                {
                    continue;
                }

                UniverseMapButton button = Instantiate(m_universeSectorButtonPrefab);
                button.SetupWaveButtons(FactoryManager.Instance.SectorRemoteData[i].GetNumberOfWaves());
                button.transform.SetParent(m_scrollRectArea.transform);
                button.transform.localPosition = rect.center + Vector2.right * 500 * i;
                button.Text.text = $"Sector {i + 1}";
                button.SectorNumber = i;
                button.Button.onClick.AddListener(() => { button.SetActiveWaveButtons(!button.ButtonsActive); });
                button.SetActiveWaveButtons(true);
                currentUniverseButtons.Add(button);
                centerOn = button.GetComponent<RectTransform>();
            }

            if (centerOn != null)
            {
                CenterToItem(centerOn);
            }
        }

        public void CenterToItem(RectTransform obj)
        {
            float normalizePositionX = ((m_scrollRectArea.rect.width / 2) + obj.anchoredPosition.x);
            float normalizePositionY = ((m_scrollRectArea.rect.height / 2) + obj.anchoredPosition.y);
            //normalizePosition += (float)obj.transform.GetSiblingIndex() / (float)m_scrollRect.content.transform.childCount;
            //normalizePosition /= 1000f;
            //normalizePosition = Mathf.Clamp01(1 - normalizePosition);
            m_scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(normalizePositionX / m_scrollRectArea.rect.width);
            m_scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizePositionY / m_scrollRectArea.rect.height);
        }

        //============================================================================================================//

        //private Vector2 GetRandomPositionInScrollRect()
        //{
        //    return new Vector2(Random.Range(m_scrollRectArea.rect.xMin, m_scrollRectArea.rect.xMax), Random.Range(m_scrollRectArea.rect.yMin, m_scrollRectArea.rect.yMax));
        //}

        //============================================================================================================//
    }
}