using StarSalvager.Missions;
using StarSalvager.Values;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard
{
    public class MissionsUI : MonoBehaviour
    {
        //============================================================================================================//
        
        [SerializeField]
        private MissionUIElementScrollView MissionUiElementScrollView;

        [SerializeField]
        private MissionUIElementScrollView MissionCompletedElementScrollView;

        [SerializeField]
        private TMP_Text detailsText;

        public static Action CheckMissionUITrackingToggles;

        //============================================================================================================//

        // Start is called before the first frame update
        private void OnEnable()
        {
            InitScrollView();
        }
        
        //============================================================================================================//

        private void InitScrollView()
        {
            MissionUiElementScrollView.ClearElements();
            MissionCompletedElementScrollView.ClearElements();


            if (MissionManager.MissionsCurrentData is null)
                return;
            
            foreach (var currentMission in MissionManager.MissionsCurrentData.CurrentMissions)
            {
                var temp = MissionUiElementScrollView.AddElement(currentMission,
                    $"{currentMission.m_missionName}_UIElement");

                temp.Init(currentMission, 
                mission =>
                {
                    detailsText.text = mission.m_missionName;
                }, 
                mission =>
                {
                    if (!PlayerPersistentData.PlayerData.missionsCurrentData.CurrentTrackedMissions.Any(m => m.m_missionName == currentMission.m_missionName))
                    {
                        if (PlayerPersistentData.PlayerData.missionsCurrentData.CurrentTrackedMissions.Count < Globals.NumCurrentTrackedMissionMax)
                        {
                            Debug.Log("Track " + mission.m_missionName);
                            PlayerPersistentData.PlayerData.missionsCurrentData.AddTrackedMissions(currentMission);
                        }
                    }
                    else
                    {
                        Debug.Log("Untrack " + mission.m_missionName);
                        PlayerPersistentData.PlayerData.missionsCurrentData.RemoveTrackedMission(currentMission);
                    }
                    CheckMissionUITrackingToggles?.Invoke();
                });
            }

            foreach (var completedMission in MissionManager.MissionsCurrentData.CompletedMissions)
            {
                var temp = MissionCompletedElementScrollView.AddElement(completedMission,
                    $"{completedMission.m_missionName}_UIElement");

                temp.Init(completedMission,
                mission =>
                {
                    detailsText.text = mission.m_missionName;
                }, null, true);
            }

            CheckMissionUITrackingToggles?.Invoke();
        }
        
        //============================================================================================================//

    }
    
    [System.Serializable]
    public class MissionUIElementScrollView: UIElementContentScrollView<MissionUIElement, Mission>
    {}
}

