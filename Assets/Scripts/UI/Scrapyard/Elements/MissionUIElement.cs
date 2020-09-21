using StarSalvager.Missions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class MissionUIElement : ButtonReturnUIElement<Mission, Mission>
    {
        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private Button favouriteButton;

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

            favouriteButton.onClick.AddListener(() =>
            {
                onTrackPressedCallback?.Invoke(data);
            });
        }
    }
}


