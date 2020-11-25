﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Audio;


#if UNITY_EDITOR

using UnityEditor;

#endif

namespace StarSalvager.Audio.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Song_Asset", menuName = "Star Salvager/Audio/Song Asset")]
    public class SongScriptableObject : ScriptableObject
    {
        private const string VOLUME = "Volume";
        
        public string TrackName => name;
        
        [OnValueChanged(nameof(CreateStems)), Required, PropertyOrder(-1000)]
        public AudioMixer mixer;

        [Title("$TrackName"), ListDrawerSettings(Expanded = true)]
        public StemData[] stems;

        [HideInInspector]
        public bool setup;

        //====================================================================================================================//
        
#if UNITY_EDITOR

        [Button, EnableIf("setup"), 
         InfoBox("Force Update is a destructive event, and will destroy all data below", "setup", InfoMessageType = InfoMessageType.Error), 
         PropertyOrder(-900)]
        private void ForceUpdate()
        {
            var response = EditorUtility.DisplayDialog("Force Update",
                "Updating the data below will remove all of its data. Are you sure you want to continue?",
                "Update", "Cancel");

            if (!response)
                return;
            
            if (!setup)
                return;

            setup = false;
            CreateStems();
        }
        private void CreateStems()
        {
            //TODO Should show some sort of warning
            if (setup)
                return;
            
            var groups = mixer.FindMatchingGroups(string.Empty);

            stems = new StemData[groups.Length - 1];

            for (int i = 0; i < groups.Length; i++)
            {
                var audioMixerGroup = groups[i];
                
                if (audioMixerGroup.name.Equals("Master"))
                    continue;
                
                
                stems[i-1] = new StemData
                {
                    name = audioMixerGroup.name,
                    MixerGroup = audioMixerGroup
                };
            }
            
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            setup = true;
        }
        
#endif

    }

    [Serializable]
    public struct StemData
    {
        public enum FADE
        {
            IN,
            OUT
        }
        
        
        public string VOLUME => $"{name}_Volume";
        
        [FoldoutGroup("$name"), DisplayAsString]
        public string name;

        [FoldoutGroup("$name")]
        public AudioMixerGroup MixerGroup;
        
        [FoldoutGroup("$name")]
        public AudioClip clip;
        
        [FoldoutGroup("$name")]
        public FadeData fadeIn;
        [FoldoutGroup("$name")]
        public FadeData fadeOut;
        
        [FoldoutGroup("$name"), Range(0f, 1f)]
        public float maxLevel;

        public float GetFadeTime(FADE fadeDirection)
        {
            switch (fadeDirection)
            {
                case FADE.IN:
                    return fadeIn.startDelay + fadeIn.time;
                case FADE.OUT:
                    return fadeOut.startDelay + fadeOut.time;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fadeDirection), fadeDirection, null);
            }
        }
        public void SetVolume(float volume)
        {
            MixerGroup.audioMixer.SetVolume(VOLUME, Mathf.Lerp(0f, maxLevel, volume));
        }
        
        public IEnumerator FadeStem(FADE fade)
        {
            FadeData fadeData;
            switch (fade)
            {
                case FADE.IN:
                    fadeData = fadeIn;
                    break;
                case FADE.OUT:
                    fadeData = fadeOut;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fade), fade, null);
            }
            
            var level = maxLevel;
            var fadeVolume = VOLUME;
            
            if(fadeData.startDelay > 0)
                yield return new WaitForSecondsRealtime(fadeData.startDelay);

            float t = 0;
            float time = fadeData.time;
            while (t / time >= 1)
            {
                var td = fadeData.curve.Evaluate(t / time);

                var vol = Mathf.Lerp(0f, level, td);

                MixerGroup.audioMixer.SetVolume(fadeVolume, vol);

                t += Time.unscaledDeltaTime;
                yield return null;
            }

        }

        
    }

    [Serializable]
    public struct FadeData
    {
        public float startDelay;
        public float time;
        public AnimationCurve curve;
    }
}
