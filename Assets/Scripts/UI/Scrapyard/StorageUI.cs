using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Saving;
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

        private DroneDesigner DroneDesigner => _droneDesigner ? _droneDesigner : (_droneDesigner = FindObjectOfType<DroneDesigner>());
        private DroneDesigner _droneDesigner;
        
        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            InitButtons();
        }

        private void OnEnable()
        {
            
            UpdateStorage();

            PlayerDataManager.OnValuesChanged += UpdateStorage;
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= UpdateStorage;   
        }

        //============================================================================================================//

        private void InitButtons()
        {

        }

        public void UpdateStorage()
        {
            //var droneDesign = FindObjectOfType<DroneDesigner>();
            
            storageUiElementScrollView.ClearElements();
            
            foreach (var storageBlockData in PlayerDataManager.GetCurrentPartsInStorage())
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
                    DroneDesigner.SelectPartFromStorage(data.blockData);
                });
            }
            
            foreach (var storageBlockData in PlayerDataManager.GetComponents())
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

