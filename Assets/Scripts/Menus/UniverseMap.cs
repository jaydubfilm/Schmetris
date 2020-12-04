using System;
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
using StarSalvager.Utilities.JsonDataTypes;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.UI;

namespace StarSalvager.UI
{
    public class UniverseMap : MonoBehaviour, IReset
    {
        private enum ICON_TYPE
        {
            WAVE,
            RING_SUM,
            RING_MAX
        }

        #region Properties
                
        [SerializeField, ReadOnly, BoxGroup("Map Stats Icon Prototyping")]
        private bool PROTO_useSum = true;
        [SerializeField, BoxGroup("Map Stats Icon Prototyping")]
        private ICON_TYPE IconType = ICON_TYPE.RING_MAX;
        

        [SerializeField, Required] private UniverseMapButton m_universeSectorButtonPrefab;

        [SerializeField, Required] private ScrollRect m_scrollRect;
        [SerializeField, Required] private RectTransform m_scrollRectArea;

        [SerializeField, Required]
        private Button swapUniverseButton;
        [SerializeField, Required]
        private Button backButton;
        [SerializeField, Required]
        private Button betweenWavesScrapyardButton;

        //====================================================================================================================//
        
        [SerializeField, FoldoutGroup("Hover Window")]
        private GameObject waveDataWindow;
        [SerializeField, FoldoutGroup("Hover Window")]
        private GameObject missingDataObject;

        [SerializeField, FoldoutGroup("Hover Window")]
        private TMP_Text windowTitle;

        [SerializeField, FoldoutGroup("Hover Window")]
        private SpriteScaleContentScrollView waveDataScrollView;
        
        private Dictionary<BIT_TYPE, float> _collectableBits;

        //====================================================================================================================//
        
        [SerializeField]
        private List<UniverseMapButton> universeMapButtons;

        [SerializeField]
        private Image dottedLineImage;

        private List<Image> _connectionLines;

        //====================================================================================================================//
        
        [SerializeField]
        private RectTransform botDisplayRectTransform;
        private List<Image> _botDisplayObjects;

        #endregion //Properties

        [SerializeField]
        private TMP_Text resourceText;

        //Unity Functions
        //============================================================================================================//

        private void Start()
        {
            InitButtons();
            _botDisplayObjects = new List<Image>();
            _connectionLines = new List<Image>();
            waveDataWindow.SetActive(false);
        }

        //IReset Functions
        //====================================================================================================================//
        
        public void Activate()
        {
            if (!GameManager.IsState(GameState.UniverseMapBetweenWaves))
            {
                GameManager.SetCurrentGameState(GameState.UniverseMapBeforeFlight);
            }
            
            backButton.gameObject.SetActive(!GameManager.IsState(GameState.UniverseMapBetweenWaves));
            betweenWavesScrapyardButton.gameObject.SetActive(GameManager.IsState(GameState.UniverseMapBetweenWaves));

            if (PROTO_useSum)
            {
                switch (IconType)
                {
                    case ICON_TYPE.WAVE:
                        PROTO_useSum = false;
                        break;
                    case ICON_TYPE.RING_SUM:
                        PROTO_useSum = true;
                        CalculateRingSum();
                        break;
                    case ICON_TYPE.RING_MAX:
                        PROTO_useSum = true;
                        CalculateRingMax();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }

            DrawMap();

            CreateBotPreview();

            resourceText.text = GetPreviewResources(PlayerDataManager.GetBlockDatas());
        }

        public void Reset()
        {
            for (int i = _connectionLines.Count - 1; i >= 0; i--)
            {
                Destroy(_connectionLines[i].gameObject);
            }
            _connectionLines.Clear();

            if (_botDisplayObjects.Count > 0)
            {
                for (int i = _botDisplayObjects.Count - 1; i >= 0; i--)
                {
                    Recycler.Recycle(typeof(Image), _botDisplayObjects[i].gameObject);
                }
                _botDisplayObjects.Clear();
            }
        }

        //UniverseMap Functions
        //====================================================================================================================//
        
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
                GameManager.SetCurrentGameState(GameState.Scrapyard);
                LevelManager.Instance.ProcessScrapyardUsageBeginAnalytics();
                LevelManager.Instance.ResetLevelTimer();
                
                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.UNIVERSE_MAP, MUSIC.SCRAPYARD);
                });
            });

            int curSector = 0;
            int curWave = 0;
            for (int i = 0; i < universeMapButtons.Count; i++)
            {
                if (i == 0)
                {
                    universeMapButtons[i].Text.text = "";
                    universeMapButtons[i].TextBelow.text = "Shipwreck";
                    continue;
                }

                universeMapButtons[i].SectorNumber = curSector;
                universeMapButtons[i].WaveNumber = curWave;
                universeMapButtons[i].Text.text = (curSector + 1) + "." + (curWave + 1);
                universeMapButtons[i].TextBelow.text = "";
                universeMapButtons[i].SetupHoveredCallback(WaveHovered);
                //int numWavesInSector = FactoryManager.Instance.SectorRemoteData[curSector].GetNumberOfWaves();
                if (curWave + 1 >= 5)
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

        private void DrawMap()
        {
            for (int i = 0; i < universeMapButtons.Count; i++)
            {
                universeMapButtons[i].Button.image.color = Color.white;
                universeMapButtons[i].BotImage.gameObject.SetActive(false);
                universeMapButtons[i].ShortcutImage.gameObject.SetActive(false);
                universeMapButtons[i].PointOfInterestImage.gameObject.SetActive(
                    PlayerDataManager.GetLevelRingNodeTree().TryFindNode(i) != null &&
                    PlayerDataManager.GetLevelRingNodeTree().TryFindNode(i).childNodes.Count == 0 &&
                    !PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Contains(i));
            }

            if (GameManager.IsState(GameState.UniverseMapBetweenWaves))
            {
                int curIndex = PlayerDataManager.GetLevelRingNodeTree()
                    .ConvertSectorWaveToNodeIndex(Globals.CurrentSector, Globals.CurrentWave);
                CenterToItem(universeMapButtons[curIndex].GetComponent<RectTransform>());

                for (int i = 0; i < PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Count; i++)
                {
                    int nodeIndex = PlayerDataManager.GetPlayerPreviouslyCompletedNodes()[i];

                    List<LevelRingNode> childNodesAccessible =
                        PlayerDataManager.GetLevelRingNodeTree().TryFindNode(nodeIndex).childNodes;

                    if (nodeIndex == curIndex)
                    {
                        for (int k = 0; k < childNodesAccessible.Count; k++)
                        {
                            bool isChildShortcut = PlayerDataManager.GetShortcutNodes()
                                .Contains(childNodesAccessible[k].nodeIndex);
                            if (isChildShortcut)
                            {
                                universeMapButtons[childNodesAccessible[k].nodeIndex].ShortcutImage.gameObject
                                    .SetActive(true);
                            }
                        }

                        universeMapButtons[nodeIndex].BotImage.gameObject.SetActive(true);
                        if (childNodesAccessible.Count == 0)
                        {
                            universeMapButtons[nodeIndex].Button.image.color = Color.red;
                            Alert.ShowAlert("Dead End", "You've reached a dead end. Return to base.", "Ok", null);
                        }

                        for (int k = 0; k < universeMapButtons.Count; k++)
                        {
                            if (childNodesAccessible.Any(n => n.nodeIndex == k))
                            {
                                universeMapButtons[k].Button.interactable = true;
                                DrawConnection(curIndex, k,
                                    !PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Any(n => n == k));
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
                            //universeMapButtons[nodeIndex].Button.image.color = Color.red;
                        }
                        else
                        {
                            for (int k = 0; k < universeMapButtons.Count; k++)
                            {
                                if (childNodesAccessible.Any(n => n.nodeIndex == k))
                                {
                                    DrawConnection(nodeIndex, k,
                                        !PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Any(n => n == k));
                                }
                            }
                        }
                    }

                    bool isShortcut = PlayerDataManager.GetShortcutNodes().Contains(nodeIndex);
                    if (isShortcut)
                    {
                        universeMapButtons[nodeIndex].ShortcutImage.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                CenterToItem(universeMapButtons[0].GetComponent<RectTransform>());
                universeMapButtons[0].BotImage.gameObject.SetActive(true);
                for (int i = 0; i < universeMapButtons.Count; i++)
                {
                    universeMapButtons[i].Button.interactable = Globals.TestingFeatures;
                }

                for (int i = 0; i < PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Count; i++)
                {
                    int nodeIndex = PlayerDataManager.GetPlayerPreviouslyCompletedNodes()[i];

                    List<LevelRingNode> childNodesAccessible =
                        PlayerDataManager.GetLevelRingNodeTree().TryFindNode(nodeIndex).childNodes;

                    for (int k = 0; k < childNodesAccessible.Count; k++)
                    {
                        bool isChildShortcut = PlayerDataManager.GetShortcutNodes()
                            .Contains(childNodesAccessible[k].nodeIndex);
                        if (isChildShortcut)
                        {
                            universeMapButtons[childNodesAccessible[k].nodeIndex].ShortcutImage.gameObject
                                .SetActive(true);
                        }
                    }

                    bool isShortcut = PlayerDataManager.GetShortcutNodes().Contains(nodeIndex);

                    if (childNodesAccessible.Count == 0)
                    {
                        //universeMapButtons[nodeIndex].Button.image.color = Color.red;
                    }
                    else
                    {
                        if (!Globals.ShortcutJumpToAfter)
                        {
                            for (int k = 0; k < childNodesAccessible.Count; k++)
                            {
                                if (PlayerDataManager.GetShortcutNodes().Contains(childNodesAccessible[k].nodeIndex))
                                {
                                    int index = childNodesAccessible[k].nodeIndex;
                                    universeMapButtons[index].Button.interactable = true;
                                    universeMapButtons[index].ShortcutImage.gameObject.SetActive(true);
                                    DrawConnection(0, index, false, true);
                                }
                            }
                        }

                        for (int k = 0; k < universeMapButtons.Count; k++)
                        {
                            if (childNodesAccessible.Any(n => n.nodeIndex == k))
                            {
                                if (nodeIndex == 0)
                                {
                                    universeMapButtons[k].Button.interactable = true;
                                }

                                DrawConnection(nodeIndex, k,
                                    !PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Any(n => n == k));
                            }
                        }
                    }

                    if (isShortcut)
                    {
                        if (Globals.ShortcutJumpToAfter)
                        {
                            for (int k = 0; k < childNodesAccessible.Count; k++)
                            {
                                int index = childNodesAccessible[k].nodeIndex;
                                universeMapButtons[index].Button.interactable = true;
                                //universeMapButtons[index].ShortcutImage.gameObject.SetActive(true);
                                //DrawConnection(0, index, false, true);
                            }
                        }

                        //universeMapButtons[nodeIndex].Button.interactable = true;
                        universeMapButtons[nodeIndex].ShortcutImage.gameObject.SetActive(true);
                    }
                }
            }

            universeMapButtons[0].Button.interactable = true;
        }

        private void DrawConnection(int connectionStart, int connectionEnd, bool dottedLine, bool colourCyan = false)
        {
            GameObject newLine;

            if (dottedLine)
            {
                newLine = Instantiate(dottedLineImage).gameObject;
            }
            else
            {
                newLine = new GameObject();
            }

            newLine.transform.parent = m_scrollRectArea.transform;
            newLine.transform.SetAsFirstSibling();

            if (!dottedLine)
            {
                newLine.AddComponent<Image>();
            }

            Image newLineImage = newLine.GetComponent<Image>();

            newLineImage.transform.position = (universeMapButtons[connectionStart].transform.position + universeMapButtons[connectionEnd].transform.position) / 2;

            if (colourCyan)
            {
                newLineImage.color = Color.cyan;
            }

            RectTransform newLineRectTransform = newLine.GetComponent<RectTransform>();
            newLineRectTransform.sizeDelta = new Vector2(Vector2.Distance(universeMapButtons[connectionStart].transform.position, universeMapButtons[connectionEnd].transform.position), 5);

            newLineRectTransform.transform.right = (universeMapButtons[connectionStart].transform.position - universeMapButtons[connectionEnd].transform.position).normalized;

            _connectionLines.Add(newLineImage);
        }

        //============================================================================================================//

        //TODO: ashulman, figure out if/why this works
        private void CenterToItem(RectTransform obj)
        {
            float normalizePositionX = ((m_scrollRectArea.rect.width / 2) + (obj.anchoredPosition.x * 2));
            float normalizePositionY = ((m_scrollRectArea.rect.height / 2) + (obj.anchoredPosition.y * 2));

            m_scrollRect.horizontalNormalizedPosition = normalizePositionX / m_scrollRectArea.rect.width;
            m_scrollRect.verticalNormalizedPosition = normalizePositionY / m_scrollRectArea.rect.height;
        }

        //Ring Sums
        //============================================================================================================//

        #region Ring Sums
        
        private void CalculateRingSum()
        {
            _collectableBits = new Dictionary<BIT_TYPE, float>();

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

                        if(!_collectableBits.ContainsKey(bitType))
                            _collectableBits.Add(bitType, 0f);

                        _collectableBits[bitType] += bit.Value;
                    }
                }
            }


            foreach (var collectable in _collectableBits)
            {
                Debug.Log($"[{collectable.Key}] = {collectable.Value}");
            }
        }
        
        private void CalculateRingMax()
        {
            _collectableBits = new Dictionary<BIT_TYPE, float>();

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

                        if(!_collectableBits.ContainsKey(bitType))
                            _collectableBits.Add(bitType, 0f);


                        _collectableBits[bitType] = Mathf.Max(_collectableBits[bitType], bit.Value);
                    }
                }
            }


            foreach (var collectable in _collectableBits)
            {
                Debug.Log($"[{collectable.Key}] = {collectable.Value}");
            }
        }

        #endregion //Ring Sums

        //Hover Preview UI
        //====================================================================================================================//

        #region Hover Preview UI

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
            windowTitle.text = $"Sector {sector + 1}.{wave + 1} data";

            if (unlocked)
            {
                //Get the actual wave data here
                var sectorData = FactoryManager.Instance.SectorRemoteData[sector];
                var (enemies, bits) = sectorData.GetIndexConvertedRemoteData(sector, wave).GetWaveSummaryData(PROTO_useSum);

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

        private IEnumerable<TEST_SpriteScale> GetSpriteTitleObjects(Dictionary<string, int> enemies, Dictionary<BIT_TYPE, float> bits)
        {
            const int SPRITE_LEVEL = 2;

            var outList = new List<TEST_SpriteScale>();
            var enemyProfile = FactoryManager.Instance.EnemyProfile;

            var bitProfile = FactoryManager.Instance.BitProfileData;

            foreach (var kvp in enemies)
            {
                outList.Add(new TEST_SpriteScale
                {
                    Sprite = enemyProfile.GetEnemyProfileData(kvp.Key).Sprite,
                    value = kvp.Value / (float)SpriteScaleUIElement.COUNT,
                });
            }

            foreach (var kvp in bits)
            {
                //Debug.Log($"[{kvp.Key}] = {kvp.Value}");
                outList.Add(new TEST_SpriteScale
                {
                    Sprite = bitProfile.GetProfile(kvp.Key).GetSprite(SPRITE_LEVEL),
                    value = kvp.Value / (PROTO_useSum ? _collectableBits[kvp.Key] : 1f)
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

        #endregion //Hover Preview UI

        //Bot Preview
        //====================================================================================================================//

        #region Bot Preview

        private void CreateBotPreview()
        {
            Image CreateImageObject(object className, object typeName, object extra = null)
            {
                var temp = new GameObject($"{className}_{typeName}{(extra != null ? $"_{extra}" : string.Empty)}");
                return temp.AddComponent<Image>();
            }
            
            Image imageObject;
            RectTransform rect;
            
            var botBlockData = PlayerDataManager.GetBlockDatas();

            var damageProfile = FactoryManager.Instance.DamageProfile;
            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
            var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            var componentFactory = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>();
            
            
            foreach (var blockData in botBlockData)
            {
                if (!Recycler.TryGrab(out imageObject))
                {
                    imageObject = CreateImageObject(blockData.ClassType, blockData.Type);
                }

                rect = (RectTransform) imageObject.transform;
                rect.SetParent(botDisplayRectTransform, false);

                BotDisplaySetPosition(rect, blockData.Coordinate.x, blockData.Coordinate.y);

                float startingHealth = 0;

                switch (blockData.ClassType)
                {
                    case nameof(Part):
                    case nameof(ScrapyardPart):
                        startingHealth = partFactory.GetRemoteData((PART_TYPE) blockData.Type).levels[blockData.Level]
                            .health;

                        imageObject.sprite = blockData.Health <= 0
                            ? FactoryManager.Instance.PartsProfileData.GetDamageSprite(blockData.Level)
                            : partFactory.GetProfileData((PART_TYPE) blockData.Type).Sprites[blockData.Level];

                        break;
                    case nameof(Bit):
                        imageObject.sprite = bitFactory.GetBitProfile((BIT_TYPE) blockData.Type).Sprites[blockData.Level];
                        startingHealth = bitFactory.GetBitRemoteData((BIT_TYPE) blockData.Type).levels[blockData.Level].health;
                        break;
                    case nameof(Component):
                        imageObject.sprite = componentFactory.GetComponentProfile((COMPONENT_TYPE) blockData.Type).Sprites[blockData.Level];
                        startingHealth = componentFactory.GetComponentRemoteData((COMPONENT_TYPE) blockData.Type).health;
                        break;
                }

                _botDisplayObjects.Add(imageObject);
                float healthPercentage = blockData.Health / startingHealth;
                
                var damageSprite = damageProfile.GetDetailSprite(healthPercentage);

                if (damageSprite == null || blockData.Health <= 0)
                    continue;

                if (!Recycler.TryGrab(out Image damageImage))
                {
                    damageImage = CreateImageObject(blockData.ClassType, blockData.Type, "Damage");
                }

                var damageRect = (RectTransform) imageObject.transform;

                damageRect.SetParent(botDisplayRectTransform, false);
                BotDisplaySetPosition(damageRect, blockData.Coordinate.x, blockData.Coordinate.y);

                damageImage.sprite = damageSprite;

                _botDisplayObjects.Add(damageImage);
            }

            if (botBlockData.Count > 0)
                return;

            if (!Recycler.TryGrab(out imageObject))
            {
                imageObject = CreateImageObject(nameof(Part), PART_TYPE.CORE);
            }

            rect = (RectTransform) imageObject.transform;
            rect.SetParent(botDisplayRectTransform, false);

            BotDisplaySetPosition(rect, 0, 0);

            imageObject.sprite = partFactory.GetProfileData(PART_TYPE.CORE).Sprites[0];

            _botDisplayObjects.Add(imageObject);
        }

        #endregion //Bot Preview

        private static string GetPreviewResources(List<BlockData> blockDatas)
        {
            var resources = CountResources(blockDatas);

            if (resources == null)
                return string.Empty;

            var outString = "Carried:\n";

            foreach (var resource in resources)
            {
                var sprite = TMP_SpriteMap.MaterialIcons[resource.Key];
                outString += $"\t{sprite} = {resource.Value}\n";
            }


            return outString;
        }

        //====================================================================================================================//

        private static Dictionary<BIT_TYPE, int> CountResources(List<BlockData> blockDatas)
        {
            if (blockDatas.IsNullOrEmpty())
                return null;
            
            var outValue = new Dictionary<BIT_TYPE, int>();
            var remoteProfile = FactoryManager.Instance.BitsRemoteData;
            
            var refineryMultiplier = PlayerDataManager.GetRefineryMultiplier();
            
            
            foreach (var bit in blockDatas.Where(x => x.ClassType.Equals(nameof(Bit))))
            {
                var bitType = (BIT_TYPE)bit.Type;
                var facilityRefiningMultiplier = PlayerDataManager.GetFacilityMultiplier(bitType);

                var remoteData = remoteProfile.GetRemoteData(bitType);
                
                if(!outValue.ContainsKey(bitType))
                    outValue.Add(bitType, 0);

                outValue[bitType] += (int) (remoteData.levels[bit.Level].resources * 
                                            refineryMultiplier *
                                            facilityRefiningMultiplier);
            }


            return outValue;
        }
        
        private static void BotDisplaySetPosition(RectTransform rect, int xOffset, int yOffset)
        {
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2Int(xOffset * 50, yOffset * 50);
            rect.sizeDelta = new Vector2(50, 50);
            rect.localScale = Vector3.one;
        }
        
    }
}
