using System;
using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using System.Collections.Generic;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Math;
using TMPro;
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


        [SerializeField, FoldoutGroup("Hover Window")]
        private GameObject waveDataWindow;
        [SerializeField, FoldoutGroup("Hover Window")]
        private GameObject missingDataObject;
        

        [SerializeField, FoldoutGroup("Hover Window")] 
        private TMP_Text windowTitle;
        [SerializeField, FoldoutGroup("Hover Window")]
        private SpriteTitleContentScrolView waveDataScrollView;

        [SerializeField]
        private List<Button> universeMapButtons;

        //============================================================================================================//

        private void Start()
        {
            InitButtons();

            waveDataWindow.SetActive(false);
        }

        public void Activate()
        {
            //InitUniverseMapTemp();
            CenterToItem(universeMapButtons[0].GetComponent<RectTransform>());

            foreach (var connection in PlayerPersistentData.PlayerData.LevelRingNodeTree.ConvertNodeTreeIntoConnections())
            {
                GameObject newLineRenderer = new GameObject();
                newLineRenderer.AddComponent<LineRenderer>();

                LineRenderer lineRenderer = newLineRenderer.GetComponent<LineRenderer>();
                lineRenderer.gameObject.transform.parent = m_scrollRect.transform;
                lineRenderer.sortingOrder = 1;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.material.color = Color.red;
                lineRenderer.startWidth = 10;
                lineRenderer.endWidth = 10;
                lineRenderer.useWorldSpace = true;
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.yellow;
                Debug.Log(connection);
                lineRenderer.SetPosition(0, universeMapButtons[connection.x].transform.position);
                lineRenderer.SetPosition(1, universeMapButtons[connection.y].transform.position);

            }

            if (PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] <= 35)
            {
                Alert.ShowAlert("Water Shortage", "You are running low on water at the base. Be sure to look for some more!", "Ok", null);
            }
        }

        public void Reset()
        {
            
        }

        //============================================================================================================//

        private void InitButtons()
        {
#if UNITY_EDITOR
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
#else
            swapUniverseButton.gameObject.SetActive(false);
#endif
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
                button.SetupWaveButtons(FactoryManager.Instance.SectorRemoteData[i].GetNumberOfWaves(), null);
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

                UniverseMapButton sectorButton = Instantiate(m_universeSectorButtonPrefab);
                sectorButton.SetupWaveButtons(FactoryManager.Instance.SectorRemoteData[i].GetNumberOfWaves(), WaveHovered);
                sectorButton.transform.SetParent(m_scrollRectArea.transform);
                sectorButton.transform.localPosition = rect.center + Vector2.right * 500 * i;
                sectorButton.Text.text = $"Sector {i + 1}";
                sectorButton.SectorNumber = i;
                sectorButton.Button.onClick.AddListener(() => { sectorButton.SetActiveWaveButtons(!sectorButton.ButtonsActive); });
                sectorButton.SetActiveWaveButtons(true);
                currentUniverseButtons.Add(sectorButton);
                centerOn = sectorButton.GetComponent<RectTransform>();
            }

            if (centerOn != null)
            {
                CenterToItem(centerOn);
            }
        }

        //TODO: ashulman, figure out if/why this works
        public void CenterToItem(RectTransform obj)
        {
            float normalizePositionX = ((m_scrollRectArea.rect.width / 2) + (obj.anchoredPosition.x * 2));
            float normalizePositionY = ((m_scrollRectArea.rect.height / 2) + (obj.anchoredPosition.y * 2));

            m_scrollRect.horizontalNormalizedPosition = normalizePositionX / m_scrollRectArea.rect.width;
            m_scrollRect.verticalNormalizedPosition = normalizePositionY / m_scrollRectArea.rect.height;
        }

        //============================================================================================================//

        private void WaveHovered(bool hovered, int sector, int wave, RectTransform rectTransform)
        {
            waveDataWindow.SetActive(hovered);

            if (!hovered)
                return;

            //See if wave is unlocked
            var unlocked = PlayerPersistentData.PlayerData.CheckIfCompleted(sector, wave);
            
            missingDataObject.SetActive(!unlocked);
            waveDataScrollView.SetActive(unlocked);

            waveDataWindow.GetComponent<VerticalLayoutGroup>().enabled = false;
            windowTitle.text = $"Sector {sector + 1} Wave {wave + 1} data";

            if (unlocked)
            {
                //Get the actual wave data here
                var sectorData = FactoryManager.Instance.SectorRemoteData[sector];
                var (enemies, bits) = sectorData.GetRemoteData(wave).GetWaveSummaryData();
            
                //Parse the information to get the sprites & titles
                var spriteTitles = GetSpriteTitleObjects(enemies, bits);
                waveDataScrollView.ClearElements();

                foreach (var spriteTitle in spriteTitles)
                {
                    var temp = waveDataScrollView.AddElement(spriteTitle);
                    temp.Init(spriteTitle);
                }
            }
            
            //Display
            StartCoroutine(ResizeRepositionCostWindowCoroutine(rectTransform));
        }

        private List<SpriteTitle> GetSpriteTitleObjects(Dictionary<string, int> Enemies, Dictionary<BIT_TYPE, float> Bits)
        {
            var outList = new List<SpriteTitle>();
            var enemyProfile = FactoryManager.Instance.EnemyProfile;
            
            var bitProfile = FactoryManager.Instance.BitProfileData;

            foreach (var kvp in Enemies)
            {
                outList.Add(new SpriteTitle
                {
                    Sprite = enemyProfile.GetEnemyProfileData(kvp.Key).Sprite,
                    Title = $"{kvp.Value}"
                });
            }

            foreach (var kvp in Bits)
            {
                outList.Add(new SpriteTitle
                {
                    Sprite = bitProfile.GetProfile(kvp.Key).GetSprite(0),
                    Title = $"{Mathf.RoundToInt(kvp.Value * 100f)}%"
                });
            }

            return outList;
        }
        
        private IEnumerator ResizeRepositionCostWindowCoroutine(RectTransform buttonTransform)
        {
            //TODO Should also reposition the window relative to the screen bounds to always keep in window
            Canvas.ForceUpdateCanvases();
            waveDataWindow.GetComponent<VerticalLayoutGroup>().enabled = true;

            yield return new WaitForEndOfFrame();

            var windowTransform = (RectTransform) waveDataWindow.transform;
            windowTransform.position = buttonTransform.position;

            //--------------------------------------------------------------------------------------------------------//

            //var pos = buttonTransform.localPosition;
            /*var sizeDelta = windowTransform.sizeDelta;
            var yDelta = sizeDelta.y / 2;
            var yBoundAbs = Screen.height / 2;

            if (pos.y + yDelta > yBoundAbs)
            {
                pos.y = yBoundAbs - yDelta;
                windowTransform.localPosition = pos;
            }
            else if (pos.y - yDelta < -yBoundAbs)
            {
                pos.y = -yBoundAbs + yDelta;
                windowTransform.localPosition = pos;
            }

            //--------------------------------------------------------------------------------------------------------//

            windowTransform.localPosition += Vector3.left * (buttonTransform.sizeDelta.x / 2f + sizeDelta.x / 2f);*/

            windowTransform.anchoredPosition += Vector2.right * 10f;
        }
    }
}