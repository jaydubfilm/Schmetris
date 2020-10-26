using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using System.Collections.Generic;
using StarSalvager.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using StarSalvager.Utilities.Saving;

namespace StarSalvager.UI
{
    public class UniverseMap : MonoBehaviour, IReset
    {
        public bool PROTO_useSum = true;

        [SerializeField, Required] private UniverseMapButton m_universeSectorButtonPrefab;

        [SerializeField, Required] private ScrollRect m_scrollRect;
        [SerializeField, Required] private RectTransform m_scrollRectArea;

        [SerializeField, Required]
        private Button swapUniverseButton;
        [SerializeField, Required]
        private Button backButton;
        [SerializeField, Required]
        private Button betweenWavesScrapyardButton;


        [SerializeField, FoldoutGroup("Hover Window")]
        private GameObject waveDataWindow;
        [SerializeField, FoldoutGroup("Hover Window")]
        private GameObject missingDataObject;


        [SerializeField, FoldoutGroup("Hover Window")]
        private TMP_Text windowTitle;
        /*[SerializeField, FoldoutGroup("Hover Window")]
        private SpriteTitleContentScrolView waveDataScrollView;*/

        [SerializeField, FoldoutGroup("Hover Window")]
        private SpriteScaleContentScrollView waveDataScrollView;

        [SerializeField]
        private List<UniverseMapButton> universeMapButtons;

        private List<Image> connectionLines = new List<Image>();

        //============================================================================================================//

        private void Start()
        {
            InitButtons();

            waveDataWindow.SetActive(false);
        }

        public void Activate()
        {
            backButton.gameObject.SetActive(!Globals.IsBetweenWavesInUniverseMap);
            betweenWavesScrapyardButton.gameObject.SetActive(Globals.IsBetweenWavesInUniverseMap);

            if(PROTO_useSum)
                CalculateRingTotalBits();
            for (int i = 0; i < universeMapButtons.Count; i++)
            {
                universeMapButtons[i].Button.image.color = Color.white;
            }

            if (Globals.IsBetweenWavesInUniverseMap)
            {
                int curIndex = PlayerDataManager.GetLevelRingNodeTree().ConvertSectorWaveToNodeIndex(Globals.CurrentSector, Globals.CurrentWave);
                CenterToItem(universeMapButtons[curIndex].GetComponent<RectTransform>());

                for (int i = 0; i < PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Count; i++)
                {
                    int nodeIndex = PlayerDataManager.GetPlayerPreviouslyCompletedNodes()[i];

                    List<LevelRingNode> childNodesAccessible = PlayerDataManager.GetLevelRingNodeTree().TryFindNode(nodeIndex).childNodes;

                    if (nodeIndex == curIndex)
                    {
                        universeMapButtons[nodeIndex].Button.image.color = Color.green;
                        for (int k = 0; k < universeMapButtons.Count; k++)
                        {
                            if (childNodesAccessible.Any(n => n.nodeIndex == k))
                            {
                                universeMapButtons[k].Button.interactable = true;
                                DrawConnection(curIndex, k, false);
                            }
                            else
                            {
                                universeMapButtons[k].Button.interactable = false;
                            }
                        }
                    }
                    else
                    {
                        if (childNodesAccessible.Count == 0)
                        {
                            universeMapButtons[nodeIndex].Button.image.color = Color.red;
                        }
                        else
                        {
                            for (int k = 0; k < universeMapButtons.Count; k++)
                            {
                                if (childNodesAccessible.Any(n => n.nodeIndex == k))
                                {
                                    DrawConnection(nodeIndex, k, true);
                                }
                            }
                        }
                    }
                }

                Globals.IsBetweenWavesInUniverseMap = false;
            }
            else
            {
                CenterToItem(universeMapButtons[0].GetComponent<RectTransform>());
                for (int i = 0; i < universeMapButtons.Count; i++)
                {
                    universeMapButtons[i].Button.interactable = !Globals.DisableTestingFeatures;
                }

                for (int i = 0; i < PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Count; i++)
                {
                    int nodeIndex = PlayerDataManager.GetPlayerPreviouslyCompletedNodes()[i];

                    List<LevelRingNode> childNodesAccessible = PlayerDataManager.GetLevelRingNodeTree().TryFindNode(nodeIndex).childNodes;

                    bool isShortcut = PlayerDataManager.GetShortcutNodes().Contains(nodeIndex);

                    if (childNodesAccessible.Count == 0)
                    {
                        universeMapButtons[nodeIndex].Button.image.color = Color.red;
                    }
                    else
                    {
                        for (int k = 0; k < universeMapButtons.Count; k++)
                        {
                            if (childNodesAccessible.Any(n => n.nodeIndex == k))
                            {
                                if (nodeIndex == 0)
                                {
                                    universeMapButtons[k].Button.interactable = true;
                                }
                                DrawConnection(nodeIndex, k, !(nodeIndex == 0 || isShortcut));
                            }
                        }
                    }

                    if (isShortcut)
                    {
                        universeMapButtons[nodeIndex].Button.interactable = true;
                        universeMapButtons[nodeIndex].Button.image.color = Color.blue;
                    }
                }
            }

            /*foreach (var connection in PlayerPersistentData.PlayerData.LevelRingNodeTree.ConvertNodeTreeIntoConnections())
            {
                DrawConnection(connection.x, connection.y);
            }*/

            if (PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource <= 35)
            {
                Alert.ShowAlert("Water Shortage", "You are running low on water at the base. Be sure to look for some more!", "Ok", null);
            }
        }

        public void Reset()
        {
            for (int i = connectionLines.Count - 1; i >= 0; i--)
            {
                Destroy(connectionLines[i].gameObject);
            }
            connectionLines.Clear();
        }

        //============================================================================================================//

        private void DrawConnection(int connectionStart, int connectionEnd, bool setRed)
        {
            GameObject newLine = new GameObject();

            newLine.transform.parent = m_scrollRectArea.transform;
            newLine.transform.SetAsFirstSibling();
            newLine.AddComponent<Image>();

            Image newLineImage = newLine.GetComponent<Image>();

            if (setRed)
            {
                newLineImage.color = Color.red;
            }

            newLineImage.transform.position = (universeMapButtons[connectionStart].transform.position + universeMapButtons[connectionEnd].transform.position) / 2;

            RectTransform newLineRectTransform = newLine.GetComponent<RectTransform>();
            newLineRectTransform.sizeDelta = new Vector2(Vector2.Distance(universeMapButtons[connectionStart].transform.position, universeMapButtons[connectionEnd].transform.position), 5);
            newLineRectTransform.transform.right = (universeMapButtons[connectionStart].transform.position - universeMapButtons[connectionEnd].transform.position).normalized;

            connectionLines.Add(newLineImage);
        }

        private void InitButtons()
        {
            /*swapUniverseButton.onClick.AddListener(() =>
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
            });*/
            swapUniverseButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(() => SceneLoader.LoadPreviousScene());

            betweenWavesScrapyardButton.onClick.AddListener(() =>
            {
                LevelManager.Instance.IsWaveProgressing = true;
                LevelManager.Instance.ProcessScrapyardUsageBeginAnalytics();
                LevelManager.Instance.EndWaveState = false;
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.UNIVERSE_MAP);
            });

            int curSector = 0;
            int curWave = 0;
            for (int i = 0; i < universeMapButtons.Count; i++)
            {
                if (i == 0)
                {
                    universeMapButtons[i].Text.text = "Home Base";
                    continue;
                }

                universeMapButtons[i].SectorNumber = curSector;
                universeMapButtons[i].WaveNumber = curWave;
                universeMapButtons[i].Text.text = (curSector + 1) + "." + (curWave + 1);
                universeMapButtons[i].SetupHoveredCallback(WaveHovered);
                int numWavesInSector = FactoryManager.Instance.SectorRemoteData[curSector].GetNumberOfWaves();
                if (curWave + 1 >= numWavesInSector)
                {
                    curSector++;
                    curWave = 0;
                    if (FactoryManager.Instance.SectorRemoteData.Count == curSector)
                    {
                        Debug.Log("SECTOR NOT EXIST " + i);
                        break;
                    }
                }
                else
                {
                    curWave++;
                }
            }
        }

        //============================================================================================================//

        //TODO: ashulman, figure out if/why this works
        public void CenterToItem(RectTransform obj)
        {
            float normalizePositionX = ((m_scrollRectArea.rect.width / 2) + (obj.anchoredPosition.x * 2));
            float normalizePositionY = ((m_scrollRectArea.rect.height / 2) + (obj.anchoredPosition.y * 2));

            m_scrollRect.horizontalNormalizedPosition = normalizePositionX / m_scrollRectArea.rect.width;
            m_scrollRect.verticalNormalizedPosition = normalizePositionY / m_scrollRectArea.rect.height;
        }

        //============================================================================================================//

        private Dictionary<BIT_TYPE, float> _collectables;
        private void CalculateRingTotalBits()
        {
            _collectables = new Dictionary<BIT_TYPE, float>();

            var sectors = FactoryManager.Instance.SectorRemoteData;

            foreach (var sector in sectors)
            {
                var waves = sector.WaveRemoteData;
                foreach (var wave in waves)
                {
                    var (_, bits) = wave.GetWaveSummaryData(true);

                    foreach (var bit in bits)
                    {
                        var bitType = bit.Key;

                        if(!_collectables.ContainsKey(bitType))
                            _collectables.Add(bitType, 0f);

                        _collectables[bitType] += bit.Value;
                    }
                }
            }


            foreach (var collectable in _collectables)
            {
                Debug.Log($"[{collectable.Key}] = {collectable.Value}");
            }
        }

        //====================================================================================================================//


        private void WaveHovered(bool hovered, int sector, int wave, RectTransform rectTransform)
        {
            waveDataWindow.SetActive(hovered);

            if (!hovered)
                return;

            //See if wave is unlocked
            int curIndex = PlayerDataManager.GetLevelRingNodeTree().ConvertSectorWaveToNodeIndex(sector, wave);
            var unlocked = PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Contains(curIndex);

            missingDataObject.SetActive(!unlocked);
            waveDataScrollView.SetActive(unlocked);

            waveDataWindow.GetComponent<VerticalLayoutGroup>().enabled = false;
            windowTitle.text = $"Sector {sector + 1} Wave {wave + 1} data";

            if (unlocked)
            {
                //Get the actual wave data here
                var sectorData = FactoryManager.Instance.SectorRemoteData[sector];
                var (enemies, bits) = sectorData.GetRemoteData(wave).GetWaveSummaryData(PROTO_useSum);

                //Parse the information to get the sprites & titles
                var testSpriteScales = GetSpriteTitleObjects(enemies, bits);
                waveDataScrollView.ClearElements();

                foreach (var spriteScale in testSpriteScales)
                {
                    var temp = waveDataScrollView.AddElement(spriteScale);
                    temp.Init(spriteScale);
                }
            }

            //Display
            StartCoroutine(ResizeRepositionCostWindowCoroutine(rectTransform));
        }

        /*private List<SpriteTitle> GetSpriteTitleObjects(Dictionary<string, int> Enemies, Dictionary<BIT_TYPE, float> Bits)
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
        }*/

        private List<TEST_SpriteScale> GetSpriteTitleObjects(Dictionary<string, int> Enemies, Dictionary<BIT_TYPE, float> Bits)
        {
            const int SPRITE_LEVEL = 2;

            var outList = new List<TEST_SpriteScale>();
            var enemyProfile = FactoryManager.Instance.EnemyProfile;

            var bitProfile = FactoryManager.Instance.BitProfileData;

            foreach (var kvp in Enemies)
            {
                outList.Add(new TEST_SpriteScale
                {
                    Sprite = enemyProfile.GetEnemyProfileData(kvp.Key).Sprite,
                    value = kvp.Value / (float)SpriteScaleUIElement.COUNT,
                });
            }

            foreach (var kvp in Bits)
            {
                Debug.Log($"[{kvp.Key}] = {kvp.Value}");
                outList.Add(new TEST_SpriteScale
                {
                    Sprite = bitProfile.GetProfile(kvp.Key).GetSprite(SPRITE_LEVEL),
                    value = kvp.Value / (PROTO_useSum ? _collectables[kvp.Key] : 1f)
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
