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

        public override void Init(Mission data, Action<Mission> OnPressed)
        {
            this.data = data;

            title.text = data.m_missionName;
            
            button.onClick.AddListener(() =>
            {
                OnPressed?.Invoke(data);
            });
        }
    }
}


