using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class FacilityBlueprintUIElement : UIElement<FacilityBlueprint>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required]
        private TMP_Text nameText;
        [SerializeField, Required]
        private Button craftButton;

        [SerializeField, Required]
        private Image stickerImage;

        private bool _canShowSticker;

        private bool _isHovered;
        private float _hoverTimer = 0;

        private Action<FacilityBlueprint, bool> _onHoverCallback;

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

            if (_hoverTimer >= 0.5f)
            {
                if (data != null)
                {
                    if (PlayerDataManager.CheckHasFacilityBlueprintAlert(data))
                    {
                        PlayerDataManager.ClearNewFacilityBlueprintAlert(data);
                        LogisticsScreenUI.CheckFacilityBlueprintNewAlertUpdate?.Invoke();
                    }
                }
            }
        }

        //============================================================================================================//

        private void OnEnable()
        {
            LogisticsScreenUI.CheckFacilityBlueprintNewAlertUpdate += OnCheckFacilityBlueprintNewAlertUpdate;
        }

        private void OnDisable()
        {
            LogisticsScreenUI.CheckFacilityBlueprintNewAlertUpdate -= OnCheckFacilityBlueprintNewAlertUpdate;
        }

        //============================================================================================================//

        public void Init(FacilityBlueprint data,
            Action<FacilityBlueprint> onCraftPressed,
            Action<FacilityBlueprint, bool> onHoverCallback, bool craftButtonInteractable, bool canShowSticker = true)
        {
            Init(data);
            _canShowSticker = canShowSticker;

            craftButton.interactable = craftButtonInteractable && PlayerDataManager.CanAffordFacilityBlueprint(data);
            stickerImage.gameObject.SetActive(_canShowSticker && PlayerDataManager.CheckHasFacilityBlueprintAlert(data));

            _onHoverCallback = onHoverCallback;

            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() => { onCraftPressed?.Invoke(this.data); });
        }

        public override void Init(FacilityBlueprint data)
        {
            this.data = data;

            nameText.text = data.name;
        }

        private void OnCheckFacilityBlueprintNewAlertUpdate()
        {
            stickerImage.gameObject.SetActive(_canShowSticker && PlayerDataManager.CheckHasFacilityBlueprintAlert(data));
        }

        //============================================================================================================//

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;

            _onHoverCallback?.Invoke(data, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;

            _onHoverCallback?.Invoke(null, false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (data != null)
            {
                if (PlayerDataManager.CheckHasFacilityBlueprintAlert(data))
                {
                    PlayerDataManager.ClearNewFacilityBlueprintAlert(data);
                    LogisticsScreenUI.CheckFacilityBlueprintNewAlertUpdate?.Invoke();
                }
            }
        }

        //============================================================================================================//
    }
}
