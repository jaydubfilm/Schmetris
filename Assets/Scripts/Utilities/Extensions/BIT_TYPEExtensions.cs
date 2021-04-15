using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    //FIXME Has to be a better way of tackling the FactoryManager.Instance search
    public static class BIT_TYPEExtensions
    {
        public static Color GetColor(this BIT_TYPE bitType) => bitType.GetProfileData().color;
        
        public static Sprite GetSprite(this BIT_TYPE bitType, in int level) => bitType.GetProfileData().GetSprite(level);

        public static BitRemoteData GetRemoteData(this BIT_TYPE bitType)
        {
#if UNITY_EDITOR
            return (FactoryManager.Instance == null
                ? Object.FindObjectOfType<FactoryManager>()
                : FactoryManager.Instance).BitsRemoteData.GetRemoteData(bitType);
#else
            return FactoryManager.Instance.BitsRemoteData.GetRemoteData(bitType);

#endif
        }

        public static BitProfile GetProfileData(this BIT_TYPE bitType)
        {
#if UNITY_EDITOR
            return (FactoryManager.Instance == null
                ? Object.FindObjectOfType<FactoryManager>()
                : FactoryManager.Instance).BitProfileData.GetProfile(bitType);
#else
            return FactoryManager.Instance.BitProfileData.GetProfile(bitType);

#endif
        }
    }
}
