using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Audio;


using UnityEditor;

namespace StarSalvager.Audio.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Song_Asset", menuName = "Star Salvager/Audio/Song Asset")]
    public class SongScriptableObject : ScriptableObject
    {
        public AudioMixer masterMixer;
        //[ShowInInspector, PropertyOrder(-1000), HideLabel]
        public string TrackName => name;

        [Title("$TrackName"), ListDrawerSettings(Expanded = true)]
        public StemData[] stems;

    }

    [Serializable]
    public struct StemData
    {
        [FoldoutGroup("$name")]
        public string name;
        
        [FoldoutGroup("$name")]
        public AudioClip clip;
        
        [FoldoutGroup("$name")]
        public FadeData fadeIn;
        [FoldoutGroup("$name")]
        public FadeData fadeOut;
        
        [FoldoutGroup("$name"), Range(0f, 1f)]
        public float maxLevel;
    }

    [Serializable]
    public struct FadeData
    {
        public float startDelay;
        public float time;
        public AnimationCurve curve;
    }
}
