using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class SpriteScaleUIElement : UIElement<TEST_SpriteScale>
    {
        public const int COUNT = 9;
        
        
        [SerializeField]
        private Image imagePrefab;

        private Image[] _images;
        
        public override void Init(TEST_SpriteScale data)
        {
            this.data = data;
            
            imagePrefab.sprite = data.Sprite;
            
            if (_images == null)
            {
                SetupImages();
            }
            
            //Show/Hide Images based on the value
            DisplayValue(data.value);
        }

        private void SetupImages()
        {
            _images = new Image[COUNT];

            for (var i = 0; i < COUNT; i++)
            {
                if (i == 0)
                {
                    _images[i] = imagePrefab;
                    continue;
                }

                var temp = Instantiate(imagePrefab, transform);
                
                _images[i] = temp;
            }
            
        }

        private void DisplayValue(float value)
        {
            var rounded = (float)Math.Round(value * COUNT, 1) - 1;
            
            for (var i = 0; i < COUNT; i++)
            {
                var active = rounded >= i + 1;
                float Dec = 0;

                if (!active)
                    Dec = rounded - i;

                if (!active && Dec >= 0.5f)
                {
                    _images[i].type = Image.Type.Filled;
                    _images[i].fillMethod = Image.FillMethod.Horizontal;
                    _images[i].fillAmount = 0.5f;
                    _images[i].enabled = true;

                }
                else
                {
                    _images[i].type = Image.Type.Simple;
                    _images[i].enabled = active;
                }
            }
        }
    }

    [Serializable]
    public struct TEST_SpriteScale: IEquatable<TEST_SpriteScale>
    {
        public Sprite Sprite;
        public float value;

        #region IEquatable

        public bool Equals(TEST_SpriteScale other)
        {
            return Sprite == other.Sprite;
        }

        public override bool Equals(object obj)
        {
            return obj is TEST_SpriteScale other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Sprite.GetHashCode();
        }

        #endregion //IEquatable
    }
    
    [Serializable]
    public class SpriteScaleContentScrollView: UIElementContentScrollView<SpriteScaleUIElement, TEST_SpriteScale>{}
}
