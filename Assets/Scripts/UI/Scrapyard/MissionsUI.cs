using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard
{
    public class MissionsUI : MonoBehaviour
    {
        private TEST_Mission[] _testMissions =
        {
            new TEST_Mission{ name = "Mission 1" }, 
            new TEST_Mission{ name = "Mission 2" }, 
            new TEST_Mission{ name = "Mission 3" }, 
            new TEST_Mission{ name = "Mission 4" }, 
            new TEST_Mission{ name = "Mission 5" }, 
        };
        
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
            foreach (var testMission in _testMissions)
            {
                var temp = MissionUiElementScrollView.AddElement<MissionUIElement>(testMission,
                    $"{testMission.name}_UIElement");

                temp.Init(testMission, mission =>
                {
                    detailsText.text = mission.name;
                });
            }
        }
        
        //============================================================================================================//

    }
    
    [System.Serializable]
    public class MissionUIElementScrollView: UIElementContentScrollView<TEST_Mission>
    {}
}

