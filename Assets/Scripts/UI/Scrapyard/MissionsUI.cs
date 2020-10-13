using StarSalvager.Missions;
using StarSalvager.Values;
using System;
using System.Linq;
using JetBrains.Annotations;
using StarSalvager.Factories;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard
{
    public class MissionsUI : MonoBehaviour
    {
        //============================================================================================================//

        [SerializeField] private MissionUIElementScrollView MissionUiElementScrollView;

        [SerializeField] private MissionUIElementScrollView MissionCompletedElementScrollView;

        [SerializeField] private TMP_Text detailsTitleText;
        [SerializeField] private TMP_Text detailsText;

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
                    $"{currentMission.missionName}_UIElement");

                temp.Init(currentMission,
                    OnHoveredChange,
                    mission =>
                    {
                        if (PlayerPersistentData.PlayerData.missionsCurrentData.CurrentTrackedMissions.All(m =>
                            m.missionName != currentMission.missionName))
                        {
                            if (PlayerPersistentData.PlayerData.missionsCurrentData.CurrentTrackedMissions.Count >=
                                Globals.NumCurrentTrackedMissionMax)
                                return;

                            Debug.Log("Track " + mission.missionName);
                            PlayerPersistentData.PlayerData.missionsCurrentData.AddTrackedMissions(currentMission);
                        }
                        else
                        {
                            Debug.Log("Untrack " + mission.missionName);
                            PlayerPersistentData.PlayerData.missionsCurrentData.RemoveTrackedMission(currentMission);
                        }

                        CheckMissionUITrackingToggles?.Invoke();
                    });
            }

            foreach (var completedMission in MissionManager.MissionsCurrentData.CompletedMissions)
            {
                var temp = MissionCompletedElementScrollView.AddElement(completedMission,
                    $"{completedMission.missionName}_UIElement");

                temp.Init(completedMission,
                    OnHoveredChange,
                    null);
            }

            CheckMissionUITrackingToggles?.Invoke();
        }

        private void OnHoveredChange([CanBeNull] Mission mission, bool isHovered)
        {
            detailsTitleText.text = isHovered ? $"Details - {mission.missionName}" : "Details";
            detailsText.text = isHovered
                ? $"{mission.missionDescription} {mission.GetMissionProgressString()}\n{mission.GetMissionRewardsString()}"
                : string.Empty;
        }

        //============================================================================================================//

    }

    [System.Serializable]
    public class MissionUIElementScrollView: UIElementContentScrollView<MissionUIElement, Mission>
    {}
}

