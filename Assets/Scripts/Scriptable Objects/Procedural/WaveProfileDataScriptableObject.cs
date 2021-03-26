using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Wave Profile Data", menuName = "Star Salvager/Scriptable Objects/Wave Profile Data")]
    public class WaveProfileDataScriptableObject : BaseMapNodeScriptableObject
    {
        public enum WAVE_TYPE
        {
            SURVIVAL,
            DEFENCE,
            BONUS,
            MINI_BOSS
        }

        [EnumToggleButtons, LabelWidth(90)] public WAVE_TYPE WaveType;

        [MinMaxSlider(10, 300), Tooltip("Time is in Seconds"), ShowIf("ShowTime")]
        public Vector2Int waveTimeRange;

        #if UNITY_EDITOR
        
        private bool ShowTime()
        {
            return WaveType == WAVE_TYPE.BONUS || WaveType == WAVE_TYPE.SURVIVAL;
        }
        
        #endif
        
    }
}
