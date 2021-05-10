﻿using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class PART_TYPEExtensions
    {
        public static BIT_TYPE GetCategory(this PART_TYPE partType) => partType.GetRemoteData().category;
        
        public static Sprite GetSprite(this PART_TYPE partType)=> partType.GetProfileData().GetSprite();

        public static PartRemoteData GetRemoteData(this PART_TYPE partType)
        {
#if UNITY_EDITOR
            return (FactoryManager.Instance == null
                ? Object.FindObjectOfType<FactoryManager>()
                : FactoryManager.Instance).PartsRemoteData.GetRemoteData(partType);
#else
            return FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType);

#endif
        }

        public static PartProfile GetProfileData(this PART_TYPE partType)
        {
#if UNITY_EDITOR
            return (FactoryManager.Instance == null
                ? Object.FindObjectOfType<FactoryManager>()
                : FactoryManager.Instance).PartsProfileData.GetProfile(partType);
#else
            return FactoryManager.Instance.PartsProfileData.GetProfile(partType);

#endif
        }
    }
}
