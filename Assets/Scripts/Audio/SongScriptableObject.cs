using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;

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
        
        //TODO Generate Mixer
        //TODO Add Mixer groups
        
        #if UNITY_EDITOR
        
        [Button]
        public void CreateMyAsset()
        {
            const string fileType = "mixer";
            const string path = "Assets/Audio/Mixer/";
            //var path = AssetDatabase.GetAssetPath(masterMixer);
            
            //TODO Should check to see if the file already exists

            var masterMixerPath = Path.Combine(path, $"MasterMixer.{fileType}");
            var newMixerPath = Path.Combine(path, $"{TrackName}.{fileType}");

            var success = AssetDatabase.CopyAsset(masterMixerPath, newMixerPath);

            
            
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            
            
            var newMixer = AssetDatabase.LoadAssetAtPath(newMixerPath, typeof(AudioMixer)) as AudioMixer;
            Selection.activeObject = newMixer;

            
            var test = newMixer.FindMatchingGroups(string.Empty);
            foreach (var mixerGroup in test)
            {
                if (mixerGroup.name.Equals("Master"))
                    continue;
                
                //Debug.Log(mixerGroup.name);
                DestroyImmediate(mixerGroup, true);
            }
            //DestroyImmediate(, true);
            //Debug.Log($"Success: {success} Name: {newMixer?.name}\nmasterMixerPath: {masterMixerPath}\nnewMixerPath: {newMixerPath}");
        }
        
        #endif
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
