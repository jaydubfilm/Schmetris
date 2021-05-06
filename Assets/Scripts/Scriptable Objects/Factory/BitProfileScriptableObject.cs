using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bit_Profile", menuName = "Star Salvager/Scriptable Objects/Bit Profile")]
    public class BitProfileScriptableObject : AttachableProfileScriptableObject<BitProfile, BIT_TYPE>
    {
        [Required, PropertyOrder(-1000)]
        public Sprite FrozenSprite;
        
        public override BitProfile GetProfile(BIT_TYPE Type)
        {
            return profiles
                .FirstOrDefault(p => p.bitType == Type);
        }
        
        public override int GetProfileIndex(BIT_TYPE Type)
        {
            return profiles.ToList().FindIndex(x => x.bitType == Type);
        }
    }
}