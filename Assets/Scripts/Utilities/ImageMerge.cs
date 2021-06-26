using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public static class PartSpriteGenerator
    {
        private static Dictionary<PART_TYPE, Sprite> _partSprites;

        public static Sprite TryGetSprite(in PART_TYPE partType)
        {
            if (_partSprites == null)
                _partSprites = new Dictionary<PART_TYPE, Sprite>();

            if (_partSprites.TryGetValue(partType, out var sprite))
                return sprite;

            var profile = FactoryManager.Instance.PartsProfileData;
            
            if(partType == PART_TYPE.EMPTY)
            {
                //Use Base Sprite
                sprite = profile.partBackground;
                _partSprites.Add(partType, sprite);
                return sprite;
            }

            var type = partType;
            var category = partType.GetCategory();
            var background = profile.borderPrototypes.FirstOrDefault(x => x.bitType == category);
            var icon = profile.partIcons.FirstOrDefault(x => x.PartType == type);

            //Get Sprite components
            sprite = ImageMerge.MergeImages(new []
                {
                    //Add Base
                    profile.partBackground,
                    //Add Outline
                    background.sprite,
                    //Add Icon
                    icon.sprite
                },
                new[]
                {
                    Color.white,
                    //Add outline Color,
                    background.color,
                    Color.white,
                }, $"GENERATED_{partType}_Sprite");
            _partSprites.Add(partType, sprite);

            return sprite;
        }
        
    }
    public static class ImageMerge
    {
        /*[SerializeField]
        private Sprite[] sprites;
        [SerializeField]
        private Texture2D[] textures;

        [SerializeField]
        private Color[] colors;

        [SerializeField, PreviewField(128)]
        private Sprite preview;

        [Button]
        private void TestMergeTextures()
        {
            preview = MergeImages(textures, colors, "Test");
        }
        
        [Button]
        private void TestMergeSprites()
        {
            preview = MergeImages(sprites.Select(x => x.texture).ToArray(), colors, "Test");
        }*/


        public static Sprite MergeImages(in Sprite[] sprites, in Color[] colors, in string name)
        {
            return MergeImages(sprites.Select(x => x.texture).ToArray(), colors, name);
        }

        public static Sprite MergeImages(in Texture2D[] textures, in Color[] colors, in string name)
        {

            //Based on: http://answers.unity.com/answers/1009449/view.html
            //--------------------------------------------------------------------------------------------------------//
            
            Color MergeColors(in Color colorA, in Color colorB)
            {
                Color B = colorA;
                Color T = colorB;
                float srcF = T.a;
                float destF = 1f - T.a;
                float alpha = srcF + destF * B.a;
                Color R = (T * srcF + B * B.a * destF)/alpha;
                R.a = alpha;
                return R;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            var (width, height) = (textures[0].width, textures[0].height);
            for (var i = 1; i < textures.Length; i++)
            {
                if (textures[i].width != width || textures[i].height != height)
                    throw new InvalidOperationException($"{nameof(MergeImages)} only works with equal sized images");
            }

            var basePixels = textures[0].GetPixels();
            var length = basePixels.Length;
            var outData = new Color[length];
            Array.Copy(basePixels, outData, length);
            
            for (var i = 1; i < textures.Length; i++)
            {
                var data = textures[i].GetPixels();
                var colorMult = colors[i];

                for (var ii = 0; ii < length; ii++)
                {
                    var baseColor = outData[ii];
                    var newColor = data[ii] * colorMult;
                    
                    outData[ii] = MergeColors(baseColor, newColor);
                }
                
            }

            var res = new Texture2D(width, height);
            res.SetPixels(outData);
            res.name = $"{name}-Texture";
            res.Apply();

            var sprite = Sprite.Create(res, new Rect(0.0f, 0.0f, width, height), new Vector2(0.5f, 0.5f), 128f);
            sprite.name = name;
            return sprite;
        }
    }
}
