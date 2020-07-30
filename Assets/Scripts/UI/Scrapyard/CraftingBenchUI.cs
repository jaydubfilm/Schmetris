using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class CraftingBenchUI : MonoBehaviour
    {
        //============================================================================================================//
        //TODO Remove this once the real blueprints are implemented
        private TEST_Blueprint[] TestBlueprints =
        {
            new TEST_Blueprint {name = "Blueprint1"},
            new TEST_Blueprint {name = "Blueprint2"},
            new TEST_Blueprint {name = "Blueprint3"},
            new TEST_Blueprint {name = "Blueprint4"},
            new TEST_Blueprint {name = "Blueprint5"},
        };
        
        //============================================================================================================//

        [SerializeField]
        private TMP_Text itemNameText;
        
        [SerializeField]
        private Image resultImage;
        
        [FormerlySerializedAs("blueprints")] [SerializeField]
        private BlueprintUIElementScrollView blueprintsContentScrollView;

        [SerializeField]
        private ResourceUIElementScrollView costContentView;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            SetupBlueprints();
        }
        
        //============================================================================================================//

        private void SetupBlueprints()
        {
            foreach (var blueprint in TestBlueprints)
            {
                var temp = blueprintsContentScrollView.AddElement<BlueprintUIElement>(blueprint, $"{blueprint.name}_UIElement");
                temp.Init(blueprint, itemName =>
                {
                    itemNameText.text = itemName;
                });
            }
            
            
        }
        
        //============================================================================================================//
    }
    
    [System.Serializable]
    public class BlueprintUIElementScrollView: UIElementContentScrollView<TEST_Blueprint>
    {}
}

