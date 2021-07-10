using System;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Parts.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard
{
    //TODO I should include the colors lerp here
    public class WreckHealthUI : MonoBehaviour
    {
        [SerializeField, Required]
        private SliderText healthSliderText;
        /*[SerializeField, Required]
        private Button repairButton;
        [SerializeField, Required]
        private TMP_Text repairButtonText;*/

        //Unity Functions
        //====================================================================================================================//
        private void OnEnable()
        {
            //PlayerDataManager.OnValuesChanged += CheckCanRepair;

            UpdateHealthBar();
            //CheckCanRepair();
        }
        
        /*private void Start()
        {
            void InitHealthBar()
            {
                var startingHealth = PlayerDataManager.GetBotMaxHealth();
            
                healthSliderText.Init(true);
                healthSliderText.SetBounds(0f, startingHealth);
            }
            
            //repairButton.onClick.AddListener(OnRepairPressed);

            InitHealthBar();
            
        }*/

        private void OnDisable()
        {
            //PlayerDataManager.OnValuesChanged -= CheckCanRepair;
        }

        //WreckHealthUI Functions
        //====================================================================================================================//

        /*private void OnRepairPressed()
        {
            RepairDrone();
            healthSliderText.value = PlayerDataManager.GetBotHealth();

            CheckCanRepair();
        }

        private void RepairDrone()
        {
            var startingHealth = PlayerDataManager.GetBotMaxHealth();
            var currentHealth = PlayerDataManager.GetBotHealth();
            
            
            var cost = Mathf.CeilToInt(startingHealth - currentHealth);
            var components = PlayerDataManager.GetGears();

            if (components == 0) throw new Exception();

            var finalCost = Mathf.Min(cost, components);
            
            
            PlayerDataManager.SubtractGears(finalCost);
            PlayerDataManager.AddRepairsDone(finalCost);


            var newHealth = Mathf.Clamp(currentHealth + finalCost, 0, startingHealth);
            
            PlayerDataManager.SetBotHealth(newHealth);
        }*/
        
        private void UpdateHealthBar()
        {

            //--------------------------------------------------------------------------------------------------------//
            
            var startingHealth = PlayerDataManager.GetBotMaxHealth();
            
            healthSliderText.Init(true);
            healthSliderText.SetBounds(0f, startingHealth);

            //--------------------------------------------------------------------------------------------------------//
            
            var health = PlayerDataManager.GetBotHealth();
            healthSliderText.value = health;
        }

        /*
        private void CheckCanRepair()
        {
            var currentHealth = PlayerDataManager.GetBotHealth();
            var startingHealth = FactoryManager.Instance.PartsRemoteData
                .GetRemoteData(PART_TYPE.CORE)
                .GetDataValue<float>(PartProperties.KEYS.Health);
            
            var canRepair = currentHealth < startingHealth;

            repairButton.gameObject.SetActive(canRepair);

            if (!canRepair)
                return;

            var cost = Mathf.CeilToInt(startingHealth - currentHealth);
            var components = PlayerDataManager.GetGears();

            var finalCost = components > 0 ? Mathf.Min(cost, components) : cost;

            repairButtonText.text = $"Repair {finalCost}{TMP_SpriteHelper.GEAR_ICON}";
            repairButton.interactable = !(finalCost > components);
        }*/

        //====================================================================================================================//
        
    }
}
