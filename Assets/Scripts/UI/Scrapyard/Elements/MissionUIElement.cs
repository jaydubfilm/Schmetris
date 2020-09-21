using StarSalvager.Missions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager;
using StarSalvager.Values;
using System.Linq;

namespace StarSalvager.UI.Scrapyard
{
    public class MissionUIElement : ButtonReturnUIElement<Mission, Mission>
    {
        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private Button favouriteButton;

        public void Update()
        {
            bool isTracked = PlayerPersistentData.PlayerData.missionsCurrentData.CurrentTrackedMissions.Any(m => m.m_missionName == data.m_missionName);

            if (isTracked)
            {
                button.image.color = Color.green;
            }
            else
            {
                button.image.color = Color.white;
            }

            favouriteButton.interactable = isTracked || PlayerPersistentData.PlayerData.missionsCurrentData.CurrentTrackedMissions.Count < Globals.NumCurrentTrackedMissionMax;
        }

        public override void Init(Mission data, Action<Mission> onPressedCallback)
        {
            this.data = data;

            title.text = data.m_missionName;
            
            button.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(data);
            });
        }

        public void Init(Mission data, Action<Mission> onPressedCallback, Action<Mission> onTrackPressedCallback)
        {
            Init(data, onPressedCallback);

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
    }
}


