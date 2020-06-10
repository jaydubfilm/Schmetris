using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Part_Profile", menuName = "Star Salvager/Scriptable Objects/Part Profile")]
    public class PartProfileScriptableObject : AttachableProfileScriptableObject<PartProfile, PART_TYPE> 
    {
        public override PartProfile GetProfile(PART_TYPE Type)
        {
            return profiles
                .FirstOrDefault(p => p.partType == Type);
        }
    }
}