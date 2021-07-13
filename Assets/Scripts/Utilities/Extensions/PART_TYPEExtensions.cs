using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.PatchTrees.Data;
using StarSalvager.Utilities.Saving;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class PART_TYPEExtensions
    {
        public static List<PatchNodeJson> GetPatchTree(this PART_TYPE partType)
        {
            var rawData = partType.GetRemoteData().patchTreeData;
            return JsonConvert.DeserializeObject<List<PatchNodeJson>>(rawData);
        }
        public static BIT_TYPE GetCategory(this PART_TYPE partType) => partType.GetRemoteData().category;
        public static Vector2Int GetCoordinateForCategory(this PART_TYPE partType) => PlayerDataManager.GetCoordinateForCategory(partType.GetRemoteData().category);
        
        /*public static bool GetRotationLocked(this PART_TYPE partType) => partType.GetRemoteData().lockRotation;*/

        public static Sprite GetPartIcon(this PART_TYPE partType) =>
            FactoryManager.Instance.PartsProfileData.partIcons.FirstOrDefault(
                x => x.PartType == partType)?.sprite;

        public static Sprite GetSprite(this PART_TYPE partType)
        {
#if UNITY_EDITOR
            var manager = FactoryManager.Instance == null
                ? Object.FindObjectOfType<FactoryManager>()
                : FactoryManager.Instance;

#else
            var manager = FactoryManager.Instance;

#endif

            //Check to see if we can use the new sprites or not, return respective sprite.
            return manager.PartsProfileData.HasNewSprite(partType)
                ? PartSpriteGenerator.TryGetSprite(partType)
                : partType.GetProfileData().GetSprite();
        }

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
