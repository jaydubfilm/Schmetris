using System;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class ComponentResourceUIElement : UIElement<ComponentAmount>
    {
        [SerializeField, Required]
        private Image componentImage;

        [SerializeField, Required]
        private TMP_Text amountText;
        [SerializeField, Required]
        private TMP_Text costText;
        
        
        
        public override void Init(ComponentAmount data)
        {
            this.data = data;

            componentImage.sprite = FactoryManager.Instance.ComponentProfile.GetProfile(data.type).GetSprite(0);
            amountText.text = $"{data.amount}";

            costText.text = string.Empty;

        }
    }

    [Serializable]
    public struct ComponentAmount : IEquatable<ComponentAmount>
    {
        public COMPONENT_TYPE type;
        public int amount;

        //This only compares Type and not all individual properties

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ComponentAmount other)
        {
            return type == other.type;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is ComponentAmount other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) type * 397) ^ amount;
            }
        }

        #endregion //IEquatable
    }
    
    [Serializable]
    public class ComponentResourceUIElementScrollView: UIElementContentScrollView<ComponentResourceUIElement, ComponentAmount>
    {}
}