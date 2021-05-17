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
using StarSalvager.AI;
using StarSalvager.Audio;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace StarSalvager.UI
{
    public class UniverseMap : MonoBehaviour, IReset, IHasHintElement
    {
        /*private enum ICON_TYPE
        {
            WAVE,
            RING_SUM,
            RING_MAX
        }*/

        #region Properties

        /*[SerializeField, ReadOnly, BoxGroup("Map Stats Icon Prototyping")]
        private bool PROTO_useSum = true;*/
        /*[SerializeField, BoxGroup("Map Stats Icon Prototyping")]
        private ICON_TYPE IconType = ICON_TYPE.RING_MAX;*/


        [FormerlySerializedAs("m_universeSectorButtonPrefab")] [SerializeField, Required]
        private UniverseMapButton universeSectorButtonPrefab;

        [SerializeField, Required] private ScrollRect m_scrollRect;
        [SerializeField, Required] private RectTransform m_scrollRectArea;

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

        //[SerializeField]
        private UniverseMapButton[] _universeMapButtons;

        [FormerlySerializedAs("dottedLineImage")] [SerializeField]
        private Image dottedLineImagePrefab;

        private List<Image> _connectionLines;

        //====================================================================================================================//

        [SerializeField] private RectTransform botDisplayRectTransform;

        //private RectTransform _shipwreckButtonRectTransform;

        #endregion //Properties

        //Unity Functions
        //============================================================================================================//

        private void Start()
        {
            InitButtons();
            _connectionLines = new List<Image>();
            waveDataWindow.SetActive(false);
        }

        //====================================================================================================================//

        public object[] GetHintElements(HINT hint)
        {
            switch (hint)
            {
                case HINT.NONE:
                    return null;
                /*case HINT.HOME:
                    return new object[]
                    {
                        _shipwreckButtonRectTransform 
                    };*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }

        //IReset Functions
        //====================================================================================================================//

        public void Activate()
        {
            /*if (PROTO_useSum)
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
                
            }*/

            DrawMap();

            PlayerDataManager.GetBlockDatas().CreateBotPreview(botDisplayRectTransform);
        }

        public void Reset()
        {
            for (int i = _connectionLines.Count - 1; i >= 0; i--)
            {
                Destroy(_connectionLines[i].gameObject);
            }

            _connectionLines.Clear();
        }



        //UniverseMap Functions
        //====================================================================================================================//

        private void InitButtons()
        {
            //--------------------------------------------------------------------------------------------------------//

            void CreateButtonElement(in int index, in int ringIndex, in NodeData nodeData)
            {
                _universeMapButtons[index] = Instantiate(universeSectorButtonPrefab, m_scrollRectArea);
                _universeMapButtons[index].Reset();
                _universeMapButtons[index].Init(index, ringIndex, nodeData);

                _universeMapButtons[index].gameObject.name = $"{nameof(UniverseMapButton)}_[{index}]";

                var sizeX = _universeMapButtons[index].transform.sizeDelta.x;
                _universeMapButtons[index].transform.anchoredPosition += Vector2.right * (index * sizeX * 1.5f);

            }

            //Get the total count of buttons we'll need to make
            //--------------------------------------------------------------------------------------------------------//

            var rings = FactoryManager.Instance.RingRemoteDatas;
            var buttonCount = rings.Sum(remoteData => remoteData.GetNodeCount());

            var nodes = new List<NodeData>();

            for (var i = 0; i < rings.Length; i++)
            {
                var tempNodes = rings[i].GetAsNodes();
                for (int ii = 0; ii < tempNodes.Count; ii++)
                {
                    tempNodes[ii].ringIndex = i;
                }
                
                nodes.AddRange(tempNodes);
            }

            //Generate the buttons
            //--------------------------------------------------------------------------------------------------------//

            _universeMapButtons = new UniverseMapButton[buttonCount];
            for (var i = 0; i < buttonCount; i++)
            {
                var nodeData = nodes[i];
                CreateButtonElement(i,nodeData.ringIndex, nodeData);
            }

            //Populate the Button Data
            //--------------------------------------------------------------------------------------------------------//


        }

        //============================================================================================================//

        public void ForceDrawMap()
        {
            DrawMap();
        }
        
        private void DrawMap()
        {
            var playerNodeLocation = PlayerDataManager.GetCurrentNode();

            CenterToItem(_universeMapButtons[playerNodeLocation].transform);

            for (var i = 0; i < _universeMapButtons.Length; i++)
            {
                var currentMapButton = _universeMapButtons[i];

                currentMapButton.SetBotImageActive(i == playerNodeLocation);

                
                //FIXME Dotted line should be dependent on the completed state of a wave
                if (i + 1 < _universeMapButtons.Length)
                    DrawConnection(i, i + 1, i + 1 > playerNodeLocation);

                var isWreck = currentMapButton.NodeType == NodeType.Wreck; //PlayerDataManager.GetWreckNodes().Contains(i);

                if (i == playerNodeLocation && isWreck)
                {
                    currentMapButton.SetButtonInteractable(true);
                    continue;
                }

                currentMapButton.SetButtonInteractable(false);

            }
            
            if (playerNodeLocation + 1 < _universeMapButtons.Length)
                _universeMapButtons[playerNodeLocation + 1].SetButtonInteractable(true);

            //Check to see if the wreck is ahead of the player and can be interacted with
            //--------------------------------------------------------------------------------------------------------//
                        
            var unlockedWreck = _universeMapButtons
                .FirstOrDefault(x => playerNodeLocation != 0 && x.IsButtonInteractable && x.NodeType == NodeType.Wreck);
            if (HintManager.CanShowHint(HINT.WRECK) && unlockedWreck != null)
            {
                HintManager.TryShowHint(HINT.WRECK, ScreenFade.DEFAULT_TIME, unlockedWreck.transform);
            }

            //--------------------------------------------------------------------------------------------------------//
            
        }

        private void DrawConnection(int connectionStart, int connectionEnd, bool dottedLine)
        {
            DrawConnection(connectionStart, connectionEnd, dottedLine, Color.white);
        }

        private void DrawConnection(in int connectionStart, in int connectionEnd, in bool dottedLine, in Color color)
        {
            var startPosition = _universeMapButtons[connectionStart].transform.position;
            var endPosition = _universeMapButtons[connectionEnd].transform.position;

            var newLineImage = dottedLine ? Instantiate(dottedLineImagePrefab) : new GameObject().AddComponent<Image>();
            newLineImage.name = $"Line_[{connectionStart}][{connectionEnd}]";
            newLineImage.color = color;


            var newLineTransform = (RectTransform) newLineImage.transform;

            newLineTransform.SetParent(m_scrollRectArea.transform);
            newLineTransform.SetAsFirstSibling();

            newLineTransform.position = (startPosition + endPosition) / 2;

            newLineTransform.sizeDelta = new Vector2(Vector2.Distance(startPosition, endPosition), 5);

            newLineTransform.right = (startPosition - endPosition).normalized;

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

        /*#region Ring Sums
        
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

        #endregion //Ring Sums*/

        //Hover Preview UI
        //====================================================================================================================//

        /*#region Hover Preview UI

        private void WaveHovered(bool hovered, int sector, int wave, RectTransform rectTransform)
        {
            /*waveDataWindow.SetActive(hovered);

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
            StartCoroutine(ResizeRepositionCostWindowCoroutine(rectTransform));#1#
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

            windowTransform.localPosition += Vector3.left * (buttonTransform.sizeDelta.x / 2f + sizeDelta.x / 2f);#1#

            windowTransform.anchoredPosition += Vector2.right * 10f;
        }

        #endregion //Hover Preview UI

        private static string GetPreviewResources(IEnumerable<IBlockData> blockDatas)
        {
            var resources = CountResources(blockDatas.OfType<BitData>().ToList());

            if (resources == null)
                return string.Empty;

            var outString = "Cargo:\n";

            foreach (var resource in resources)
            {
                var sprite = TMP_SpriteMap.MaterialIcons[resource.Key];
                outString += $"\t{sprite} = {resource.Value}\n";
            }


            return outString;
        }*/

        //====================================================================================================================//

        /*private static Dictionary<BIT_TYPE, int> CountResources(List<BitData> blockDatas)
        {
            if (blockDatas.IsNullOrEmpty())
                return null;
            
            var outValue = new Dictionary<BIT_TYPE, int>();
            var remoteProfile = FactoryManager.Instance.BitsRemoteData;
            
            
            foreach (var bit in blockDatas)
            {
                var bitType = (BIT_TYPE)bit.Type;

                var remoteData = remoteProfile.GetRemoteData(bitType);
                
                if(!outValue.ContainsKey(bitType))
                    outValue.Add(bitType, 0);

                outValue[bitType] += remoteData.levels[bit.Level].resources;
            }


            return outValue;
        }*/
    }
}
