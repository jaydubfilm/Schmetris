using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class BIT_TYPEExtensions
    {
        public static Color GetColor(this BIT_TYPE bitType)
        {
            return FactoryManager.Instance.BitProfileData.GetProfile(bitType).color;
        }
        public static BitRemoteData GetRemoteData(this BIT_TYPE bitType)
        {
            return FactoryManager.Instance.BitsRemoteData.GetRemoteData(bitType);
        }
        public static BitProfile GetProfileData(this BIT_TYPE bitType)
        {
            return FactoryManager.Instance.BitProfileData.GetProfile(bitType);
        }
    }
}
