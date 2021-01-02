using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Black Hole Remote", menuName = "Star Salvager/Scriptable Objects/Black Hole Remote Data")]
    public class BlackHoleRemoteDataScriptableObject : ScriptableObject
    {
        public float BlackHoleMaxPull => blackHoleMaxPull;
        public float BlackHoleMaxDistance => _blackHoleMaxDistance;

        [SerializeField]
        private float blackHoleMaxPull;
        [SerializeField]
        private float _blackHoleMaxDistance;
    }
}

