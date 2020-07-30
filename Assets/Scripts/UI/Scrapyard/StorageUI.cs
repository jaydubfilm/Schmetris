using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class StorageUI : MonoBehaviour
    {
        private TEST_Storage[] _storage =
        {
            new TEST_Storage { name = "Item 1" },
            new TEST_Storage { name = "Item 2" }, 
            new TEST_Storage { name = "Item 3" }, 
            new TEST_Storage { name = "Item 4" }, 
            new TEST_Storage { name = "Item 5" }, 
        };
        
        private TEST_Storage[] _inventory =
        {
            new TEST_Storage { name = "Inv 1" },
            new TEST_Storage { name = "Inv 2" }, 
            new TEST_Storage { name = "Inv 3" }, 
            new TEST_Storage { name = "Inv 4" }, 
            new TEST_Storage { name = "Inv 5" }, 
        };
        
        //============================================================================================================//
        
        //TODO These need to be set up to function as a tab
        [SerializeField, Required]
        private Button storageButton;
        [SerializeField, Required]
        private Button inventoryButton;
        
        [SerializeField]
        private StorageUIElementScrollView storageUiElementScrollView;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            InitButtons();

            InitContent();
        }
        
        //============================================================================================================//

        private void InitButtons()
        {
            storageButton.onClick.AddListener(() =>
            {
                
            });
            
            inventoryButton.onClick.AddListener(() =>
            {
                
            });
        }

        private void InitContent()
        {
            foreach (var testStorage in _storage)
            {
                var temp = storageUiElementScrollView.AddElement<StorageUIElement>(testStorage, $"{testStorage.name}_UIElement");
                
                temp.Init(testStorage);

            }
        }
        
        //============================================================================================================//

    }
    
    [System.Serializable]
    public class StorageUIElementScrollView: UIElementContentScrollView<TEST_Storage>
    {}
}

