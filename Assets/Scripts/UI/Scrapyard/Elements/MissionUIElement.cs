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
    public class MissionUIElement : UIElement<Mission>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required]
        private Image elementImage;
        
        [SerializeField, Required]
        private TMP_Text title;

        [SerializeField, Required]
        private Button favouriteButton;

        private Action<Mission, bool> _onHoverCallback;

        //Unity Functions
        //====================================================================================================================//
        
        public void OnEnable()
        {
            MissionsUI.CheckMissionUITrackingToggles += OnCheckMissionUITrackingToggles;
        }

        public void OnDisable()
        {
            MissionsUI.CheckMissionUITrackingToggles -= OnCheckMissionUITrackingToggles;
        }

        //====================================================================================================================//
        
        public void OnCheckMissionUITrackingToggles()
        {
            bool isTracked =
                PlayerDataManager.GetMissionsCurrentData().CurrentTrackedMissions.Any(m =>
                    m.missionName == data.missionName && !m.MissionComplete());

            elementImage.color = isTracked ? Color.green : Color.white;

            favouriteButton.interactable = isTracked || PlayerDataManager.GetMissionsCurrentData().CurrentTrackedMissions.Count < Globals.NumCurrentTrackedMissionMax;
        }

        //Init Functions
        //====================================================================================================================//

        public void Init(Mission data, Action<Mission, bool> onHoverCallback, Action<Mission> onTrackPressedCallback)
        {
            Init(data);

            _onHoverCallback = onHoverCallback;

            var shouldTrack = onTrackPressedCallback != null;
            
            favouriteButton.gameObject.SetActive(shouldTrack);

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
            _onHoverCallback?.Invoke(data, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverCallback?.Invoke(null, false);
        }

        //====================================================================================================================//
        
    }
}


