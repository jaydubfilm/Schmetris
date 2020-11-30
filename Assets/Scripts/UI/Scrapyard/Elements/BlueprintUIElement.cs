using StarSalvager.Factories;
using System;
using Sirenix.OdinInspector;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Saving;

namespace StarSalvager.UI.Scrapyard
{
    public class BlueprintUIElement : UIElement<Blueprint>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private Image image;

        [SerializeField, Required]
        private Button craftButton;

        [SerializeField, Required]
        private Image stickerImage;

        private bool _canShowSticker;

        private bool _isHovered;
        private float _hoverTimer = 0;

        private Action<Blueprint, bool, RectTransform> hoverCallback;

        //============================================================================================================//

        public void Update()
        {
            if (_isHovered)
            {
                _hoverTimer += Time.deltaTime;
            }
            else
            {
                _hoverTimer = 0;
            }

            if (_hoverTimer >= 1)
            {
                if (data != null)
                {
                    if (PlayerDataManager.CheckHasBlueprintAlert(data))
                    {
                        PlayerDataManager.ClearNewBlueprintAlert(data);
                        MissionsUI.CheckBlueprintNewAlertUpdate?.Invoke();
                    }
                }
            }
        }

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += UpdateUI;
            MissionsUI.CheckBlueprintNewAlertUpdate += OnCheckBlueprintNewAlertUpdate;
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= UpdateUI;
            MissionsUI.CheckBlueprintNewAlertUpdate -= OnCheckBlueprintNewAlertUpdate;
        }

        //============================================================================================================//

        public void Init(Blueprint data, Action<Blueprint> OnCraftPressed, Action<Blueprint, bool, RectTransform> OnHover, bool canShowSticker = true)
        {
            Init(data);

            hoverCallback = OnHover;
            _canShowSticker = canShowSticker;
            //craftButtonImage = craftButton.GetComponent<Image>();

            craftButton.interactable = Globals.TestingFeatures || data.CanAfford;
            stickerImage.gameObject.SetActive(_canShowSticker && PlayerDataManager.CheckHasBlueprintAlert(data));

            /*if (PlayerPersistentData.PlayerData.CanAffordPart(data.partType, data.level, false))
                craftButtonImage.color = craftButton.colors.normalColor;
            else
                craftButton.GetComponent<Image>().color = craftButton.colors.disabledColor;*/

            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                OnCraftPressed?.Invoke(data);
            });
        }

        public override void Init(Blueprint data)
        {
            try
            {
                this.data = data;

                titleText.text = data.name;
            
                //Only try and fill the image in the event its enabled
                if(image.isActiveAndEnabled)
                    image.sprite = FactoryManager.Instance
                        .GetFactory<PartAttachableFactory>()
                        .GetProfileData(data.partType)
                        .GetSprite(data.level);
            }
            catch (NullReferenceException)
            {
                Debug.LogError($"Cannot find profile or sprite for {data.partType} of level {data.level}");
                throw;
            }
        }

        private void OnCheckBlueprintNewAlertUpdate()
        {
            stickerImage.gameObject.SetActive(_canShowSticker && PlayerDataManager.CheckHasBlueprintAlert(data));
        }

        //============================================================================================================//

        private void UpdateUI()
        {
            craftButton.interactable = Globals.TestingFeatures || data.CanAfford;
            /*if (PlayerPersistentData.PlayerData.CanAffordPart(data.partType, data.level, false))
                craftButtonImage.color = craftButton.colors.normalColor;
            else
                craftButton.GetComponent<Image>().color = craftButton.colors.disabledColor;*/
        }
        
        //============================================================================================================//
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;

            hoverCallback?.Invoke(data, true, transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;

            hoverCallback?.Invoke(null, false, transform);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (data != null)
            {
                if (PlayerDataManager.CheckHasBlueprintAlert(data))
                {
                    PlayerDataManager.ClearNewBlueprintAlert(data);
                    MissionsUI.CheckBlueprintNewAlertUpdate?.Invoke();
                }
            }
        }

        //============================================================================================================//
    }
}
