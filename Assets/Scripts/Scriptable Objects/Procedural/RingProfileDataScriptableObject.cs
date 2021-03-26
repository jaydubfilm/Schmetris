using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Ring Data", menuName = "Star Salvager/Scriptable Objects/Ring Data")]
    public class RingProfileDataScriptableObject : ScriptableObject
    {
        [Serializable]
        public struct WaveSpawnData
        {
            public WaveProfileDataScriptableObject.WAVE_TYPE WaveType;

            [AssetList(CustomFilterMethod = "HasRigidbodyComponent")]
            public List<WaveProfileDataScriptableObject> waves;

            [MinMaxSlider(1,10, true)]
            public Vector2Int count;
            
            private bool HasRigidbodyComponent(WaveProfileDataScriptableObject obj)
            {
                return obj.WaveType == WaveProfileDataScriptableObject.WAVE_TYPE.SURVIVAL;
            }

        }
        
        [MinMaxSlider(3, 10, true)]
        public Vector2Int sectorsRange = Vector2Int.one;
        [MinMaxSlider(1, 5, true)]
        public Vector2Int nodesRange = Vector2Int.one;

        [MinMaxSlider(1, 3, true), ReadOnly]
        public Vector2Int pathsRange = Vector2Int.one;

        [InlineEditor]
        public List<WreckNodeDataScriptableObject> wrecks;
        [InlineEditor]
        public List<WaveProfileDataScriptableObject> waves;


        [BoxGroup("Survival Waves")]
        public WaveSpawnData test = new WaveSpawnData
        {
            WaveType = WaveProfileDataScriptableObject.WAVE_TYPE.SURVIVAL
        };


    }
}
