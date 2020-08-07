using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class UniverseMap : MonoBehaviour
    {
        [SerializeField, Required] private UniverseMapButton m_universeSectorButtonPrefab;

        //[SerializeField, Required]
        //private Canvas m_universeCanvas;

        [SerializeField, Required] private RectTransform m_scrollRectArea;

        [SerializeField, Required]
        private Button backButton;

        //============================================================================================================//

        private void Start()
        {
            InitUniverseMapTemp();

            InitButtons();
        }

        //============================================================================================================//

        private void InitButtons()
        {
            backButton.onClick.AddListener(() => SceneLoader.LoadPreviousScene());
        }

        //============================================================================================================//

        private void InitUniverseMap()
        {
            HaltonSequence positionSequence = new HaltonSequence();

            Rect rect = m_scrollRectArea.rect;

            Vector2 size = new Vector2(rect.xMax - rect.xMin, rect.yMax - rect.yMin);
            positionSequence.Reset();

            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
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
            }
        }

        private void InitUniverseMapTemp()
        {
            Rect rect = m_scrollRectArea.rect;
            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
                UniverseMapButton button = Instantiate(m_universeSectorButtonPrefab);
                button.SetupWaveButtons(FactoryManager.Instance.SectorRemoteData[i].GetNumberOfWaves());
                button.transform.SetParent(m_scrollRectArea.transform);
                button.transform.localPosition = rect.center + Vector2.right * 400 * i;
                button.Text.text = $"Sector {i + 1}";
                button.SectorNumber = i;
                button.Button.onClick.AddListener(() => { button.SetActiveWaveButtons(!button.ButtonsActive); });
                button.SetActiveWaveButtons(true);
            }
        }

        //============================================================================================================//

        //private Vector2 GetRandomPositionInScrollRect()
        //{
        //    return new Vector2(Random.Range(m_scrollRectArea.rect.xMin, m_scrollRectArea.rect.xMax), Random.Range(m_scrollRectArea.rect.yMin, m_scrollRectArea.rect.yMax));
        //}

        //============================================================================================================//
    }
}