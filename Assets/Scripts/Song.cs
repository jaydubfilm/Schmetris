using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Audio.ScriptableObjects;
using UnityEngine;

namespace StarSalvager.Audio
{
    public class Song : MonoBehaviour
    {
        public MUSIC Music;

        [ShowInInspector, ProgressBar(0f, 1f, 0.15f, 0.74f, 0.47f), ReadOnly]
        public float CurrentVolume { get; private set; }

        [ShowInInspector, ReadOnly] 
        public bool IsPlaying => CurrentVolume > 0f;

        [SerializeField, Required]
        private SongScriptableObject song;

        /*[SerializeField] private bool startMuted = true;*/

        private bool _fading;

        //Volume Functions
        //====================================================================================================================//
        
        
        [Button, DisableInEditorMode, HorizontalGroup("Row1")]
        public void SetFullVolume()
        {
            foreach (var stem in song.stems)
            {
                stem.SetVolume(1f);
            }

            CurrentVolume = 1f;
        }
        [Button, DisableInEditorMode, HorizontalGroup("Row1")]
        public void Mute()
        {
            foreach (var stem in song.stems)
            {
                stem.SetVolume(0f);
            }

            CurrentVolume = 0f;
            //IsPlaying = false;
        }

        //Fade Functions
        //====================================================================================================================//
        [Button, DisableInEditorMode, HorizontalGroup("Row2")]
        public void FadeInTrack()
        {
            if (_fading)
                return;

            if (CurrentVolume >= 1f)
                return;

            StartCoroutine(FadeTrack(StemData.FADE.IN));
        }
        [Button, DisableInEditorMode, HorizontalGroup("Row2")]
        public void FadeOutTrack()
        {
            if (_fading)
                return;
            
            if (CurrentVolume == 0f)
                return;

            StartCoroutine(FadeTrack(StemData.FADE.OUT));
        }

        //Coroutines
        //====================================================================================================================//
        
        private  IEnumerator FadeTrack(StemData.FADE fadeDirection)
        {
            _fading = true;
            
            var maxFadeTime = song.stems.Max(s => s.GetFadeTime(fadeDirection));
            var stems = song.stems;

            foreach (var t in stems)
            {
                StartCoroutine(FadeStemCoroutine(t, fadeDirection));
            }

            float start, target;
            
            switch (fadeDirection)
            {
                case StemData.FADE.IN:
                    start = 0f;
                    target = 1f;
                    break;
                case StemData.FADE.OUT:
                    start = 1f;
                    target = 0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fadeDirection), fadeDirection, null);
            }

            var wait = 0f;
            while (wait <= maxFadeTime)
            {

                CurrentVolume = Mathf.Lerp(start, target, wait / maxFadeTime);
                
                wait += Time.unscaledDeltaTime;
                
                yield return null;
            }

            CurrentVolume = target;

            //IsPlaying = fadeDirection == StemData.FADE.IN;
            
            _fading = false;
        }
        
        private static IEnumerator FadeStemCoroutine(StemData stemData, StemData.FADE fade)
        {
            FadeData fadeData;
            switch (fade)
            {
                case StemData.FADE.IN:
                    fadeData = stemData.fadeIn;
                    break;
                case StemData.FADE.OUT:
                    fadeData = stemData.fadeOut;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fade), fade, null);
            }
            
            var level = stemData.maxLevel;
            //var fadeVolume = stemData.VOLUME;
            
            if(fadeData.startDelay > 0)
                yield return new WaitForSecondsRealtime(fadeData.startDelay);

            float t = 0;
            float time = fadeData.time;
            while (t / time <= 1f)
            {
                var td = fadeData.curve.Evaluate(t / time);

                var vol = Mathf.Lerp(0f, level, td);

                stemData.SetVolume(vol);

                t += Time.unscaledDeltaTime;
                
                yield return null;
            }

        }

        //====================================================================================================================//
        
        [Button, DisableInPlayMode]
        private void SetupObject()
        {
            gameObject.name = $"{song.TrackName}_TrackSource";
            foreach (var stemData in song.stems)
            {
                var temp = new GameObject($"{stemData.name}AudioSource");
                var audioSource = temp.AddComponent<AudioSource>();

                audioSource.loop = true;
                audioSource.outputAudioMixerGroup = stemData.MixerGroup;
                audioSource.clip = stemData.clip;
                
                temp.transform.SetParent(transform, false);
            }
        }

        //====================================================================================================================//
        
    }
}
