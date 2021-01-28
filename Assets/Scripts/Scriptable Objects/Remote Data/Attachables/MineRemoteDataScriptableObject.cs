using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Mine Remote", menuName = "Star Salvager/Scriptable Objects/Mine Remote Data")]
    public class MineRemoteDataScriptableObject : ScriptableObject
    {
        public float MineMaxDamage => _mineMaxDamage;
        public float MineMaxDistance => _mineMaxDistance;

        [SerializeField]
        private float _mineMaxDamage = 15.0f;
        [SerializeField]
        private float _mineMaxDistance = 4.0f;
    }
}

