using Sirenix.OdinInspector;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI
{
    public class GameHealthUI : MonoBehaviour
    {
        [SerializeField, Required]
        private RectTransform healthBarTransform;
        [SerializeField, Required]
        private RectTransform sliderTransform;
        [SerializeField, Required]
        private RectTransform fillAreaTransform;
        
        [SerializeField]
        private float minSizeDeltaX;
        [SerializeField]
        private float maxSizeDeltaX;

        [SerializeField, Required] private TMP_Text healthText;

        private Vector2 _maxHealthBounds;

        private float _currentMaxHealth;

        //====================================================================================================================//
        private void OnEnable()
        {
            _maxHealthBounds = Globals.MaxHealthBounds;
            PlayerDataManager.OnHealthChanged += SetHealth;
            SetHealth(PlayerDataManager.GetBotHealth(), PlayerDataManager.GetBotMaxHealth());
        }

        private void OnDisable()
        {
            PlayerDataManager.OnHealthChanged -= SetHealth;
        }

        //====================================================================================================================//


        private void SetHealth(float currentHealth, float maxHealth)
        {
            _currentMaxHealth = Mathf.Clamp(maxHealth, _maxHealthBounds.x, _maxHealthBounds.y);
            

            var maxValue = Mathf.InverseLerp(_maxHealthBounds.x, _maxHealthBounds.y, _currentMaxHealth);
            
            
            var sizeDelta = healthBarTransform.sizeDelta;
            sizeDelta.x = Mathf.Lerp(minSizeDeltaX, maxSizeDeltaX, maxValue);
            
            healthBarTransform.sizeDelta = sizeDelta;

            SetHealth(currentHealth);
        }

        private void SetHealth(float currentHealth)
        {
            if (currentHealth < 0)
                currentHealth = 0;
            
            var fillWidth = fillAreaTransform.rect.width - 5f;
            var value = currentHealth / _currentMaxHealth;
            sliderTransform.sizeDelta = new Vector2(fillWidth * value, 0f);
            //healthSlider.value = value;
            healthText.text = $"{currentHealth:#0}/{_currentMaxHealth:#0}";
        }
    }
}
