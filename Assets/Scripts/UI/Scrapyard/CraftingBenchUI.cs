using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class CraftingBenchUI : MonoBehaviour
    {
        [SerializeField, Required]
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;

        [SerializeField]
        private Button craftButton;

        [SerializeField]
        private TMP_Text itemNameText;
        [SerializeField]
        private Image resultImage;

        private TEST_Blueprint selectedBlueprint;


        [FormerlySerializedAs("blueprints")] [SerializeField]
        private BlueprintUIElementScrollView blueprintsContentScrollView;
        [SerializeField]
        private ResourceUIElementScrollView costContentView;
        [SerializeField]
        private ResourceUIElementScrollView resourceScrollView;

        //============================================================================================================//

        [SerializeField, Required]
        private CraftingBench mCraftingBench;

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            InitButtons();
        }

        private void InitButtons()
        {
            craftButton.onClick.AddListener(() =>
            {
                if (selectedBlueprint == null)
                    return;

                mCraftingBench.CraftBlueprint(selectedBlueprint);
                UpdateResources(PlayerPersistentData.PlayerData.GetResources());
            });

        }

        void OnEnable()
        {
            blueprintsContentScrollView.ClearElements<BlueprintUIElement>();
            InitUIScrollViews();
        }

        //============================================================================================================//

        private void InitUIScrollViews()
        {
            //FIXME This needs to move to the Factory
            foreach (var partRemoteData in _remotePartProfileScriptable.partRemoteData)
            {
                for (int i = 0; i < partRemoteData.levels.Count; i++)
                {
                    if (partRemoteData.partType == PART_TYPE.CORE && i == 0)
                        continue;

                    TEST_Blueprint blueprint = new TEST_Blueprint
                    {
                        name = partRemoteData.partType + " " + i,
                        remoteData = partRemoteData,
                        level = i
                    };

                    var temp = blueprintsContentScrollView.AddElement<BlueprintUIElement>(blueprint, $"{blueprint.name}_UIElement");
                    temp.Init(blueprint, BlueprintPressed);
                }
            }

            var resources = PlayerPersistentData.PlayerData.GetResources();
            foreach (var resource in resources)
            {
                var data = new CraftCost
                {
                    resourceType = CraftCost.TYPE.Bit,
                    type = (int)resource.Key,
                    amount = resource.Value
                };

                var element = resourceScrollView.AddElement<ResourceUIElement>(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }
            UpdateResources(resources);
        }

        private void SetupBlueprintCosts(TEST_Blueprint blueprint)
        {
            var resources = blueprint.remoteData.levels[blueprint.level].cost;

            costContentView.ClearElements<ResourceUIElement>();
            foreach (var resource in resources)
            {
                var element = costContentView.AddElement<ResourceUIElement>(resource, $"{resource.type}_UIElement");
                element.Init(resource);
            }
        }

        public void UpdateResources(Dictionary<BIT_TYPE, int> resources)
        {
            UpdateResources(resources.ToResourceList());
        }

        public void UpdateResources(List<ResourceAmount> resources)
        {
            foreach (var resourceAmount in resources)
            {
                var data = new CraftCost
                {
                    resourceType = CraftCost.TYPE.Bit,
                    type = (int)resourceAmount.type,
                    amount = resourceAmount.amount
                };

                var element = resourceScrollView.FindElement<ResourceUIElement>(data);

                if (element == null)
                    continue;

                element.Init(data);
            }
        }

        private void BlueprintPressed(TEST_Blueprint blueprint)
        {
            itemNameText.text = blueprint.name;
            PartProfile partProfile = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(blueprint.remoteData.partType);
            resultImage.sprite = partProfile.Sprites[blueprint.level];
            SetupBlueprintCosts(blueprint);
            selectedBlueprint = blueprint;
        }

        //============================================================================================================//
        }
    
    [System.Serializable]
    public class BlueprintUIElementScrollView: UIElementContentScrollView<TEST_Blueprint>
    {}
}

