using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;


namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Component_Profile", menuName = "Star Salvager/Scriptable Objects/Component Profile")]
    public class ComponentProfileScriptableObject : AttachableProfileScriptableObject<ComponentProfile, COMPONENT_TYPE>
    {
        public override ComponentProfile GetProfile(COMPONENT_TYPE Type)
        {
            return profiles
                .FirstOrDefault(p => p.componentType == Type);
        }
        
        public override int GetProfileIndex(COMPONENT_TYPE Type)
        {
            return profiles.ToList().FindIndex(x => x.componentType == Type);
        }
    }
}
