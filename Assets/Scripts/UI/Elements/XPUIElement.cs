using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Elements
{
    //====================================================================================================================//
    
    public struct XPData : IEquatable<XPData>
    {
        public Sprite Sprite;
        public int Count;
        public int XpPerCount;

        #region IEquatable

        public bool Equals(XPData other)
        {
            return Equals(Sprite, other.Sprite);
        }

        public override bool Equals(object obj)
        {
            return obj is XPData other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = Sprite != null ? Sprite.GetHashCode() : 0;
            return hashCode;
        }

        #endregion //IEquatable
    }

    //====================================================================================================================//
    
    public class XPUIElement : UIElement<XPData>
    {
        [SerializeField]
        private Image image;
        [SerializeField]
        private TMP_Text countText;
        [SerializeField]
        private TMP_Text xpText;

        //====================================================================================================================//
        
        public override void Init(XPData data)
        {
            this.data = data;

            image.sprite = data.Sprite;
            countText.text = string.Empty;
            xpText.text = string.Empty;
            /*countText.text = $"x{data.Count}";
            xpText.text = $"+{data.XpPerCount}xp";*/
        }

        public void SetCount(in int count)
        {
            countText.text = $"x{count}";
        }

        public void SetXP(in int xp)
        {
            xpText.text = $"+{xp}xp";
        }

        //====================================================================================================================//
        
    }
    
    [Serializable]
    public class XPUIElementScrollView : UIElementContentScrollView<XPUIElement, XPData>
    {
    }
    
}
