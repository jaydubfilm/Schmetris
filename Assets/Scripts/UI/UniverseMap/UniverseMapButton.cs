using System;
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
using StarSalvager.AI;
using StarSalvager.UI;
using StarSalvager.Utilities.Interfaces;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    public enum NodeType
    {
        Base,
        Level,
        Wreck
    }

    [RequireComponent(typeof(Button)), RequireComponent(typeof(PointerEvents))]
    public class UniverseMapButton : MonoBehaviour
    {
        //Properties
        //====================================================================================================================//

        public Button Button => button;
        
        #region Properties

        private static UniverseMap _universeMap;

        public bool IsButtonInteractable => Button.interactable;
        
        public NodeType NodeType => nodeType;
        public int NodeIndex => nodeIndex;
        
        [SerializeField, ReadOnly]
        private NodeType nodeType;

        [SerializeField, ReadOnly] private int nodeIndex;

        [FormerlySerializedAs("Button")] [SerializeField] private Button button;
        [SerializeField] private Image foregroundImage;

        [SerializeField] private Image BotImage;
        
        private Image[] _wreckTypeImages;
        private Image[] _wreckLevelImages;

        //Sprites
        //====================================================================================================================//
        
        [SerializeField, FoldoutGroup("Sprites")]
        private Sprite wreckBackgroundSprite;
        [SerializeField, FoldoutGroup("Sprites")]
        private Sprite waveBackgroundSprite;

        [SerializeField, BoxGroup("Sprites/Waves")]
        private Sprite waveSprite;
        [SerializeField, BoxGroup("Sprites/Waves")]
        private Sprite defenceWaveSprite;
        [SerializeField, BoxGroup("Sprites/Waves")]
        private Sprite bonusWaveSprite;
        [SerializeField, BoxGroup("Sprites/Waves")]
        private Sprite bossWaveSprite;
        [SerializeField, BoxGroup("Sprites/Waves")]
        private Sprite wildcardWaveSprite;
        
        [SerializeField, BoxGroup("Sprites/Wrecks")]
        private Sprite wreckSprite;
        [SerializeField, BoxGroup("Sprites/Wrecks")]
        private Sprite wreckWildcardSprite;
        [SerializeField, BoxGroup("Sprites/Wrecks")]
        private Sprite wreckLevelSprite;
        [SerializeField, BoxGroup("Sprites/Wrecks")]
        private Sprite[] wreckBitSprites;

        //====================================================================================================================//
        
        public new RectTransform transform
        {
            get
            {
                if(_transform == null)
                    _transform = gameObject.transform as RectTransform;

                return _transform;
            }
        }
        private RectTransform _transform;

        #endregion //Properties

        //====================================================================================================================//

        public void Init(in int nodeIndex, in int sector, in NodeType nodeType, Action<int, NodeType> onPressedCallback)
        {
            if (!_universeMap) _universeMap = FindObjectOfType<UniverseMap>();
            
            Button.onClick.RemoveAllListeners();
            
            BotImage.sprite = PART_TYPE.CORE.GetSprite();

            this.nodeIndex = nodeIndex;
            this.nodeType = nodeType;
            
            Button.onClick.AddListener(() => onPressedCallback?.Invoke(this.nodeIndex, this.nodeType));

            SetupSprites();
        }

        private void SetButtonProperties(in bool buttonInteractable, in Color color)
        {
            SetButtonColor(color);
            SetButtonInteractable(buttonInteractable);
        }
        private void SetButtonColor(in Color buttonColor)
        {
            Button.image.color = buttonColor;
        }
        public void SetButtonInteractable(in bool buttonInteractable)
        {
            Button.interactable = buttonInteractable;

            foregroundImage.color = buttonInteractable ? Button.colors.normalColor : Button.colors.disabledColor;
        }


        private void SetupSprites()
        {

            //--------------------------------------------------------------------------------------------------------//
            
            void SetupWreckTypeImages()
            {
                const float DEGREE_OFFSET = 0;
                const float DEGREE_SPREAD = 180;

                var size = ((RectTransform) Button.image.transform).sizeDelta.x * 0.9f;

                var count = wreckBitSprites.Length;
                var degIncrement = DEGREE_SPREAD / (count - 1);
                _wreckTypeImages = new Image[count];
                
                for (int i = 0; i < count; i++)
                {
                    var point = Mathfx.GetAsPointOnCircle((i * degIncrement) + DEGREE_OFFSET, size / 2);

                    var temp = new GameObject(wreckBitSprites[i].name);
                    var image = temp.AddComponent<Image>();
                    temp.transform.SetParent(Button.transform);
                    temp.transform.SetSiblingIndex(0);
                    ((RectTransform)temp.transform).sizeDelta = Vector2.one * 17f;

                    image.sprite = wreckBitSprites[i];
                    image.raycastTarget = false;

                    temp.transform.localPosition = point;

                    _wreckTypeImages[i] = image;
                }
            }

            void SetupWreckLevelImages()
            {
                const float DEGREE_SPREAD = 100;
                const int LEVELS = 3;

                var size = ((RectTransform) Button.image.transform).sizeDelta.x * 0.9f;

                var degIncrement = DEGREE_SPREAD / LEVELS;
                _wreckLevelImages = new Image[LEVELS];

                var degree_offset = 270 - degIncrement;
                
                for (var i = 0; i < LEVELS; i++)
                {
                    var point = Mathfx.GetAsPointOnCircle((i * degIncrement) + degree_offset, size / 2);

                    var temp = new GameObject($"{wreckLevelSprite.name}_{i + 1}");
                    var image = temp.AddComponent<Image>();
                    temp.transform.SetParent(Button.transform);
                    temp.transform.SetSiblingIndex(0);
                    ((RectTransform)temp.transform).sizeDelta = Vector2.one * 20f;

                    image.sprite = wreckLevelSprite;
                    image.raycastTarget = false;

                    temp.transform.localPosition = point;

                    _wreckLevelImages[i] = image;
                }
            }

            void ShowWreckSprites(in int level, in BIT_TYPE[] bitTypes)
            {
                for (var i = 0; i < _wreckLevelImages.Length; i++)
                {
                    _wreckLevelImages[i].gameObject.SetActive(i <= level);
                }

                var bitList = bitTypes.Select(x => (int)x).ToList();
                for (var i = 0; i < _wreckTypeImages.Length; i++)
                {
                    var type = i + 1;
                    
                    _wreckTypeImages[i].gameObject.SetActive(bitList.Contains(type));
                }
            }

            //--------------------------------------------------------------------------------------------------------//
            
            Sprite backgroundSprite;
            Sprite foregroundSprite;

            switch (NodeType)
            {
                
                case NodeType.Level:
                    backgroundSprite = waveBackgroundSprite;
                    // Change from: https://agamestudios.atlassian.net/browse/SS-187
                    foregroundSprite = Random.value <= 0.25f ? wildcardWaveSprite : waveSprite;
                    break;
                case NodeType.Base:
                    foregroundSprite = null;
                    backgroundSprite = wreckBackgroundSprite;
                    break;
                case NodeType.Wreck:
                    if(_wreckTypeImages.IsNullOrEmpty()) 
                        SetupWreckTypeImages();
                    if(_wreckLevelImages.IsNullOrEmpty()) 
                        SetupWreckLevelImages();
                    backgroundSprite = wreckBackgroundSprite;
                    foregroundSprite = wreckSprite;

                    //Random Test Values
                    //--------------------------------------------------------------------------------------------------------//
                    
                    var level = Random.Range(0, 3);
                    var count = Random.Range(1, 6);
                    var types = new BIT_TYPE[count];
                    for (var i = 0; i < count; i++)
                    {
                        types[i] = (BIT_TYPE) i + 1;
                    }

                    //--------------------------------------------------------------------------------------------------------//
                    
                    ShowWreckSprites(level, types);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Button.image.sprite = backgroundSprite;
            foregroundImage.sprite = foregroundSprite;
            foregroundImage.gameObject.SetActive(foregroundSprite != null);
        }

        public void SetBotImageActive(in bool state)
        {
            BotImage.gameObject.SetActive(state);
        }

        public void Reset()
        {
            SetButtonProperties(false, Color.white);
            SetBotImageActive(false);
        }

        //====================================================================================================================//

    }
}