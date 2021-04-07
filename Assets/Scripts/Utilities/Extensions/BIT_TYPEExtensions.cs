using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    //FIXME Has to be a better way of tackling the FactoryManager.Instance search
    public static class BIT_TYPEExtensions
    {
        public static Color GetColor(this BIT_TYPE bitType)
        {
#if UNITY_EDITOR
            return (FactoryManager.Instance == null
                    ? Object.FindObjectOfType<FactoryManager>()
                    : FactoryManager.Instance)
                .BitProfileData.GetProfile(bitType).color;
#else
            return FactoryManager.Instance.BitProfileData.GetProfile(bitType).color;
#endif

        }

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
