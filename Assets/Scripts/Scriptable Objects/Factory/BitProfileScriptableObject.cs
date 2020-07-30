using System.Linq;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bit_Profile", menuName = "Star Salvager/Scriptable Objects/Bit Profile")]
    public class BitProfileScriptableObject : AttachableProfileScriptableObject<BitProfile, BIT_TYPE>
    {
        
        public override BitProfile GetProfile(BIT_TYPE Type)
        {
            return profiles
                .FirstOrDefault(p => p.bitType == Type);
        }
    }
}