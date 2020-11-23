using StarSalvager.Missions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager.Values;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using StarSalvager.Utilities.Saving;

namespace StarSalvager.UI.Scrapyard
{
    public class MissionUIElement : UIElement<Mission>, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField, Required]
        private Image elementImage;
        
        [SerializeField, Required]
        private TMP_Text title;

        [SerializeField, Required]
        private TMP_Text buttonText;

        [SerializeField, Required]
        private Button favouriteButton;

        [SerializeField, Required]
        private Image stickerImage;

        private Action<Mission, bool> _onHoverCallback;

        private bool _canShowSticker;

        private bool _isHovered;
        private float _hoverTimer = 0;

        //Unity Functions
        //====================================================================================================================//

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
                    if (PlayerDataManager.CheckHasMissionAlert(data))
                    {
                        PlayerDataManager.ClearNewMissionAlert(data);
                        MissionsUI.CheckMissionNewAlertUpdate?.Invoke();
                    }
                }
            }
        }

        public void OnEnable()
        {
            MissionsUI.CheckMissionUITrackingToggles += OnCheckMissionUITrackingToggles;
            MissionsUI.CheckMissionNewAlertUpdate += OnCheckMissionNewAlertUpdate;
        }

        public void OnDisable()
        {
            MissionsUI.CheckMissionUITrackingToggles -= OnCheckMissionUITrackingToggles;
            MissionsUI.CheckMissionNewAlertUpdate -= OnCheckMissionNewAlertUpdate;
        }

        //====================================================================================================================//
        
        public void OnCheckMissionUITrackingToggles()
        {
            bool isTracked =
                PlayerDataManager.GetMissionsCurrentData().CurrentTrackedMissions.Any(m =>
                    m.missionName == data.missionName && !m.MissionComplete());

            elementImage.color = isTracked ? Color.green : Color.white;

            buttonText.text = isTracked ? "Untrack" : "Track";

            //favouriteButton.interactable = isTracked || PlayerDataManager.GetMissionsCurrentData().CurrentTrackedMissions.Count < Globals.NumCurrentTrackedMissionMax;
            favouriteButton.interactable = true;
        }

        private void OnCheckMissionNewAlertUpdate()
        {
            stickerImage.gameObject.SetActive(_canShowSticker && PlayerDataManager.CheckHasMissionAlert(data));
        }

        //Init Functions
        //====================================================================================================================//

        public void Init(Mission data, Action<Mission, bool> onHoverCallback, Action<Mission> onTrackPressedCallback, bool canShowSticker = true)
        {
            Init(data);

            _onHoverCallback = onHoverCallback;
            _canShowSticker = canShowSticker;

            var shouldTrack = onTrackPressedCallback != null;
            
            favouriteButton.gameObject.SetActive(shouldTrack);
            stickerImage.gameObject.SetActive(_canShowSticker && PlayerDataManager.CheckHasMissionAlert(data));

            if (!shouldTrack)
                return;
            
            favouriteButton.onClick.RemoveAllListeners();
            favouriteButton.onClick.AddListener(() =>
            {
                onTrackPressedCallback?.Invoke(data);
            });
            
        }
        
        public override void Init(Mission data)
        {
            this.data = data;

            title.text = data.missionName;
            
            //button.onClick.AddListener(() =>
            //{
            //    onPressedCallback?.Invoke(data);
            //});
        }

        //IPointer Events
        //====================================================================================================================//
        
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
                if (PlayerDataManager.CheckHasMissionAlert(data))
                {
                    PlayerDataManager.ClearNewMissionAlert(data);
                    MissionsUI.CheckMissionNewAlertUpdate?.Invoke();
                }
            }
        }

        //====================================================================================================================//
        
    }
}


