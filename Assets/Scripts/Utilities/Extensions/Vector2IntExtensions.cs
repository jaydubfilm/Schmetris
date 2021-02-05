using Recycling;
using StarSalvager.Factories;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.Extensions
{
    public static class Vector2IntExtensions
    {
        private static readonly Vector2Int[] DirectionVectors = {
            new Vector2Int(-1, 0),   //LEFT
            new Vector2Int(0, 1),    //UP
            new Vector2Int(1, 0),    //RIGHT
            new Vector2Int(0, -1)    //DOWN
        };
        
        public static Vector2Int ToVector2Int(this DIRECTION direction)
        {
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.UP:
                case DIRECTION.RIGHT:
                case DIRECTION.DOWN:
                    return DirectionVectors[(int) direction];
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        public static DIRECTION Reflected(this DIRECTION direction)
        {
            switch (direction)
            {
                case DIRECTION.LEFT:
                    return DIRECTION.RIGHT;
                case DIRECTION.UP:
                    return DIRECTION.DOWN;
                case DIRECTION.RIGHT:
                    return DIRECTION.LEFT;
                case DIRECTION.DOWN:
                    return DIRECTION.UP;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        public static Vector2Int Reflected(this Vector2Int direction)
        {
            return direction * -1;
        }
        public static DIRECTION ToDirection(this Vector2Int vector2Int)
        {
            if(vector2Int == Vector2Int.zero)
                //throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");
                return DIRECTION.NULL;
            
            if (vector2Int.x != 0 && vector2Int.y != 0)
                return DIRECTION.NULL;

            if (vector2Int.x == 0)
            {
                return vector2Int.y > 0 ? DIRECTION.UP : DIRECTION.DOWN;
            }
            
            if (vector2Int.y == 0)
            {
                return vector2Int.x > 0 ? DIRECTION.RIGHT : DIRECTION.LEFT;
            }
            
            //throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");
            return DIRECTION.NULL;
        }

        public static bool TryParseVector2Int(string s, out Vector2Int value)
        {
            value = Vector2Int.zero;

            try
            {
                //Trim all the unnecessary fat
                s = s.Replace("(", "").Replace(")", "").Replace(" ", "");

                var split = s.Split(',');

                value.x = int.Parse(split[0]);
                value.y = int.Parse(split[1]);

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void CreateBotPreview(this List<Vector2Int> coordinates, in RectTransform containerRect)
        {
            Transform[] allChildren = containerRect.GetComponentsInChildren<Transform>();
            if (allChildren.Length > 0)
            {
                for (int i = allChildren.Length - 1; i >= 0; i--)
                {
                    if (allChildren[i] == containerRect.transform)
                    {
                        continue;
                    }

                    Image image = allChildren[i].GetComponent<Image>();
                    if (image != null)
                    {
                        Recycler.Recycle<Image>(image);
                    }
                    else
                    {
                        GameObject.Destroy(allChildren[i]);
                    }
                }
            }

            if (coordinates == null)
            {
                return;
            }

            Image CreateImageObject(object className, object typeName, object extra = null)
            {
                var temp = new GameObject($"{className}_{typeName}{(extra != null ? $"_{extra}" : string.Empty)}");
                return temp.AddComponent<Image>();
            }

            void BotDisplaySetPosition(RectTransform newImageRect, int xOffset, int yOffset)
            {
                newImageRect.pivot = new Vector2(0.5f, 0.5f);
                newImageRect.anchoredPosition = new Vector2Int(xOffset * 50, yOffset * 50);
                newImageRect.sizeDelta = new Vector2(50, 50);
                newImageRect.localScale = Vector3.one;
            }

            Image imageObject;
            RectTransform rect;
            Transform botDisplayRectTransform = containerRect.transform;

            var damageProfile = FactoryManager.Instance.DamageProfile;
            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();


            foreach (var coordinate in coordinates)
            {
                if (!Recycler.TryGrab(out imageObject))
                {
                    imageObject = CreateImageObject(typeof(Part), typeof(PART_TYPE));
                }

                rect = (RectTransform)imageObject.transform;
                rect.SetParent(botDisplayRectTransform, false);

                BotDisplaySetPosition(rect, coordinate.x, coordinate.y);

                imageObject.sprite = partFactory.GetProfileData(PART_TYPE.EMPTY).GetSprite();
                /*if (coordinate == Vector2Int.zero)
                {
                    imageObject.sprite = partFactory.GetProfileData(PART_TYPE.CORE).GetSprite();
                }
                else
                {
                    imageObject.sprite = partFactory.GetProfileData(PART_TYPE.EMPTY).GetSprite();
                }*/
            }

            if (coordinates.Count > 0)
                return;

            if (!Recycler.TryGrab(out imageObject))
            {
                imageObject = CreateImageObject(nameof(Part), PART_TYPE.EMPTY);
            }

            rect = (RectTransform)imageObject.transform;
            rect.SetParent(botDisplayRectTransform, false);

            BotDisplaySetPosition(rect, 0, 0);

            imageObject.sprite = partFactory.GetProfileData(PART_TYPE.EMPTY).GetSprite();
        }
    }
}
