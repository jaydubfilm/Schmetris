using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Component Remote", menuName = "Star Salvager/Scriptable Objects/Component Remote Data")]
    public class ComponentRemoteDataScriptableObject : ScriptableObject
    {
        public List<ComponentRemoteData> ComponentRemoteData = new List<ComponentRemoteData>();

        private Dictionary<COMPONENT_TYPE, ComponentRemoteData> data;

        public ComponentRemoteData GetRemoteData(COMPONENT_TYPE Type)
        {
            if (data == null)
            {
                data = new Dictionary<COMPONENT_TYPE, ComponentRemoteData>();
            }

            if (!data.ContainsKey(Type))
            {
                data.Add(Type, ComponentRemoteData
                    .FirstOrDefault(p => p.componentType == Type));
            }

            return data[Type];
        }
    }
}
