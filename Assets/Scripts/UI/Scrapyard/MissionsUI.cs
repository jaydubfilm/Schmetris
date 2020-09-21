using StarSalvager.Missions;
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
        private TMP_Text detailsText;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void OnEnable()
        {
            InitScrollView();
        }
        
        //============================================================================================================//

        private void InitScrollView()
        {
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
                    Debug.Log("Track " + mission.m_missionName);
                });
            }
        }
        
        //============================================================================================================//

    }
    
    [System.Serializable]
    public class MissionUIElementScrollView: UIElementContentScrollView<MissionUIElement, Mission>
    {}
}

