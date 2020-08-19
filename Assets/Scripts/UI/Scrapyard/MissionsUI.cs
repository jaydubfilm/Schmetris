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
        private void Start()
        {
            InitScrollView();
        }
        
        //============================================================================================================//

        private void InitScrollView()
        {
            foreach (var testMission in MissionManager.MissionsCurrentData.CurrentMissions)
            {
                var temp = MissionUiElementScrollView.AddElement<MissionUIElement>(testMission,
                    $"{testMission.m_missionName}_UIElement");

                temp.Init(testMission, mission =>
                {
                    detailsText.text = mission.m_missionName;
                });
            }
        }
        
        //============================================================================================================//

    }
    
    [System.Serializable]
    public class MissionUIElementScrollView: UIElementContentScrollView<Mission>
    {}
}

