using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class SpriteTitleUIElement : UIElement<SpriteTitle>
    {
        [SerializeField] private Image Image;
        [SerializeField] private TMP_Text titleText;


        public override void Init(SpriteTitle data)
        {
            this.data = data;

            Image.sprite = data.Sprite;
            titleText.text = data.Title;
        }
    }

    public struct SpriteTitle : IEquatable<SpriteTitle>
    {
        public Sprite Sprite;
        public string Title;

        #region IEquatable

        public bool Equals(SpriteTitle other)
        {
            return Equals(Sprite, other.Sprite) && Title == other.Title;
        }

        public override bool Equals(object obj)
        {
            return obj is SpriteTitle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Sprite != null ? Sprite.GetHashCode() : 0) * 397) ^ (Title != null ? Title.GetHashCode() : 0);
            }
        }

        #endregion //IEquatable
    }

    [Serializable]
    public class SpriteTitleContentScrolView : UIElementContentScrollView<SpriteTitleUIElement, SpriteTitle>
    {
    }
}
