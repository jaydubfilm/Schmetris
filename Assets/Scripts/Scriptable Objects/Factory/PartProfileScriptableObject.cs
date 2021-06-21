using System;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Part_Profile", menuName = "Star Salvager/Scriptable Objects/Part Profile")]
    public class PartProfileScriptableObject : AttachableProfileScriptableObject<PartProfile, PART_TYPE>
    {
        [Serializable]
        private struct PartBorder
        {
            public BIT_TYPE bitType;
            public Sprite sprite;
        }
        
        //Properties
        //====================================================================================================================//
        
        [SerializeField, PropertyOrder(-100), Space(10f)]
        private Sprite[] damagedSprites;

        public Sprite GetDamageSprite(int level)
        {
            return damagedSprites[level];
        }
        
        public Sprite EmptySprite => emptySprite;
        
        [SerializeField, PropertyOrder(-10)]
        private Sprite emptySprite;

        //====================================================================================================================//

        [TableList, SerializeField, PropertyOrder(-1000)]
        private PartBorder[] partBorders;

        public Sprite GetPartBorder(PART_TYPE partType) => partBorders.FirstOrDefault(x => x.bitType == partType.GetCategory()).sprite;
        public Sprite GetPartBorder(BIT_TYPE bitType) => partBorders.FirstOrDefault(x => x.bitType == bitType).sprite;
        
        //====================================================================================================================//
        
        public override PartProfile GetProfile(PART_TYPE Type)
        {
            return profiles
                .FirstOrDefault(p => p.partType == Type);
        }

        public override int GetProfileIndex(PART_TYPE Type)
        {
            return profiles.ToList().FindIndex(x => x.partType == Type);
        }
    }
}