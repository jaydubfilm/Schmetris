using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class StorageUI : MonoBehaviour
    {
        /*private TEST_Storage[] _storage =
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
        };*/
        
        //============================================================================================================//
        
        /*//TODO These need to be set up to function as a tab
        [SerializeField, Required]
        private Button storageButton;
        [SerializeField, Required]
        private Button inventoryButton;*/
        
        [SerializeField]
        private StorageUIElementScrollView storageUiElementScrollView;

        //============================================================================================================//

        [SerializeField, Required]
        private Storage mStorage;

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            /*List<BlockData> blockData = new List<BlockData>();
            blockData.Add(new BlockData
            {
                Level = 0,
                Type = (int)PART_TYPE.GUN,
                ClassType = "Part"
            });
            blockData.Add(new BlockData
            {
                Level = 1,
                Type = (int)PART_TYPE.MAGNET,
                ClassType = "Part"
            });
            blockData.Add(new BlockData
            {
                Level = 1,
                Type = (int)PART_TYPE.REPAIR,
                ClassType = "Part"
            });
            PlayerPersistentData.PlayerData.SetCurrentPartsInStorage(blockData);*/

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
            /*storageButton.onClick.AddListener(() =>
            {
                
            });
            
            inventoryButton.onClick.AddListener(() =>
            {
                
            });*/
        }

        /*private void InitContent()
        {
            foreach (var storageBlockData in PlayerPersistentData.PlayerData.GetCurrentPartsInStorage())
            {
                TEST_Storage testStorage = new TEST_Storage
                {
                    name = (PART_TYPE)storageBlockData.Type + " " + storageBlockData.Level,
                    blockData = storageBlockData
                };

                var temp = storageUiElementScrollView.AddElement<StorageUIElement>(testStorage, $"{testStorage.name}_UIElement", allowDuplicate: true);
                temp.Init(testStorage);
            }
        }*/

        public void UpdateStorage()
        {
            var droneDesign = FindObjectOfType<DroneDesigner>();
            
            storageUiElementScrollView.ClearElements<StorageUIElement>();
            
            foreach (var storageBlockData in PlayerPersistentData.PlayerData.GetCurrentPartsInStorage())
            {
                TEST_Storage testStorage = new TEST_Storage
                {
                    name = (PART_TYPE)storageBlockData.Type + " " + storageBlockData.Level,
                    sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData((PART_TYPE)storageBlockData.Type).GetSprite(storageBlockData.Level),
                    blockData = storageBlockData
                };

                var temp = storageUiElementScrollView.AddElement<StorageUIElement>(testStorage, $"{testStorage.name}_UIElement", allowDuplicate: true);
                temp.Init(testStorage, data =>
                {
                    droneDesign.selectedPartType = (PART_TYPE) data.blockData.Type;
                    droneDesign.SelectedPartLevel = data.blockData.Level;
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
                    var temp = storageUiElementScrollView.AddElement<StorageUIElement>(testStorage, $"{testStorage.name}_UIElement", allowDuplicate: true);
                    temp.Init(testStorage, null); 
                }
            }
        }

        //============================================================================================================//

    }
    
    [System.Serializable]
    public class StorageUIElementScrollView: UIElementContentScrollView<TEST_Storage>
    {}
}

