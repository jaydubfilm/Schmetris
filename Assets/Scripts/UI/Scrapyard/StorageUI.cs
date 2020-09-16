using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard
{
    public class StorageUI : MonoBehaviour
    {
        
        //============================================================================================================//
        
        [SerializeField]
        private StorageUIElementScrollView storageUiElementScrollView;

        //============================================================================================================//

        [SerializeField, Required]
        private Storage mStorage;

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            InitButtons();
        }

        private void OnEnable()
        {
            
            UpdateStorage();

            PlayerData.OnValuesChanged += UpdateStorage;
        }

        private void OnDisable()
        {
            PlayerData.OnValuesChanged -= UpdateStorage;   
        }

        //============================================================================================================//

        private void InitButtons()
        {

        }

        public void UpdateStorage()
        {
            var droneDesign = FindObjectOfType<DroneDesigner>();
            
            storageUiElementScrollView.ClearElements();
            
            foreach (var storageBlockData in PlayerPersistentData.PlayerData.GetCurrentPartsInStorage())
            {
                TEST_Storage testStorage = new TEST_Storage
                {
                    name = (PART_TYPE)storageBlockData.Type + " " + storageBlockData.Level,
                    sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData((PART_TYPE)storageBlockData.Type).GetSprite(storageBlockData.Level),
                    blockData = storageBlockData
                };

                var temp = storageUiElementScrollView.AddElement(testStorage, $"{testStorage.name}_UIElement", allowDuplicate: true);
                temp.Init(testStorage, data =>
                {
                    droneDesign.SelectPartFromStorage(data.blockData);
                });
            }
            
            foreach (var storageBlockData in PlayerPersistentData.PlayerData.components)
            {
                //TODO Need to separate the components
                TEST_Storage testStorage = new TEST_Storage
                {
                    name = storageBlockData.Key.ToString(),
                    sprite = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>().GetComponentProfile(storageBlockData.Key).GetSprite(0),
                };

                for (int i = 0; i < storageBlockData.Value; i++)
                {
                    var temp = storageUiElementScrollView.AddElement(testStorage, $"{testStorage.name}_UIElement", allowDuplicate: true);
                    temp.Init(testStorage, null); 
                }
            }
        }

        //============================================================================================================//

    }
    
    [System.Serializable]
    public class StorageUIElementScrollView: UIElementContentScrollView<StorageUIElement, TEST_Storage>
    {}
}

