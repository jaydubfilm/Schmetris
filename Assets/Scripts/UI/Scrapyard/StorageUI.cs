using System;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.UI.Scrapyard
{
    public class StorageUI : MonoBehaviour, IHasHintElement
    {

        //============================================================================================================//

        [SerializeField] private StorageUIElementScrollView storageUiElementScrollView;

        [SerializeField, BoxGroup("Patches")] 
        private PatchUIElementScrollView patchUIElementScrollView;

        //============================================================================================================//

        private DroneDesigner DroneDesigner =>
            _droneDesigner ? _droneDesigner : _droneDesigner = FindObjectOfType<DroneDesigner>();

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
            var partProfiles = FactoryManager.Instance.PartsProfileData;
            var partRemoteData = FactoryManager.Instance.PartsRemoteData;
            var bitProfiles = FactoryManager.Instance.BitProfileData;
            
            // Update the Part Storage Scroll View contents
            //--------------------------------------------------------------------------------------------------------//
            storageUiElementScrollView.ClearElements();
            var storedParts = PlayerDataManager.GetCurrentPartsInStorage();
            for (int i = 0; i < storedParts.Count; i++)
            {
                var storageBlockData = storedParts[i];
                var type = (PART_TYPE) storageBlockData.Type;

                var sprite = partProfiles.GetProfile(type).GetSprite(0);
                var category = partRemoteData.GetRemoteData(type).category;
                var color = bitProfiles.GetProfile(category).color;

                int tempInt = i;
                TEST_Storage testStorage = new TEST_Storage
                {
                    name = $"{type}",
                    sprite = sprite,
                    color = color,
                    blockData = storageBlockData,
                    storageIndex = tempInt
                };

                var temp = storageUiElementScrollView.AddElement(testStorage, $"{testStorage.name}_UIElement",
                    allowDuplicate: true);
                temp.Init(testStorage, data => { DroneDesigner.SelectPartFromStorage(data.blockData, tempInt); });
            }

            // Update the Patch Scroll View contents
            //--------------------------------------------------------------------------------------------------------//

            patchUIElementScrollView.ClearElements();
            var storedPatches = PlayerDataManager.GetCurrentPatchesInStorage();
            for (int i = 0; i < storedPatches.Count; i++)
            {
                var storageBlockData = storedPatches[i];

                int tempInt = i;
                Patch_Storage patchStorage = new Patch_Storage
                {
                    PatchData = storageBlockData,
                    storageIndex = tempInt
                };

                var temp = patchUIElementScrollView.AddElement(patchStorage,
                    $"{(PATCH_TYPE) storageBlockData.Type}_UIElement", 
                    allowDuplicate: true);

                temp.Init(patchStorage);
            }

        }

        //============================================================================================================//

        public object[] GetHintElements(HINT hint)
        {
            switch (hint)
            {
                case HINT.NONE:
                    return null;
                /*case HINT.CRAFT_PART:
                    return new object[]
                    {
                        storageUiElementScrollView.Elements.FirstOrDefault(x =>
                            x.data.blockData.ClassType.Equals(nameof(Part)))?.transform
                    };*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }

#if UNITY_EDITOR

        [Button]
        private void AddPatchToStorage()
        {
            PlayerDataManager.AddPatchToStorage(new PatchData
            {
                Type = (int)PATCH_TYPE.DAMAGE,
                Level = Random.Range(0, 5)
            });
        }
        
#endif
    }

    [System.Serializable]
    public class StorageUIElementScrollView : UIElementContentScrollView<StorageUIElement, TEST_Storage>
    {
    }
}

