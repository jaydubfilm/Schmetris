using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Part_Profile", menuName = "Star Salvager/Scriptable Objects/Part Profile")]
    public class PartProfileScriptableObject : AttachableProfileScriptableObject<PartProfile, PART_TYPE>
    {
        [SerializeField, PropertyOrder(-100)]
        private Sprite[] damagedSprites;

        public Sprite GetDamageSprite(int level)
        {
            return damagedSprites[level];
        }
        
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