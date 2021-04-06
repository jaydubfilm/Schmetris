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

        [ShowInInspector, ReadOnly] public bool IsPlaying => CurrentVolume > 0f;

        [SerializeField, Required] private SongScriptableObject song;

        /*[SerializeField] private bool startMuted = true;*/

        //private bool _fading;

        private float _targetVolume;
        private Coroutine _coroutine;

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
            //Debug.Log($"Fade In {Music}");
            
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                ForceComplete();
                _coroutine = null;
            }

            if (CurrentVolume >= 1f)
                return;

            _coroutine = StartCoroutine(FadeTrack(StemData.FADE.IN));
        }

        [Button, DisableInEditorMode, HorizontalGroup("Row2")]
        public void FadeOutTrack()
        {
            //Debug.Log($"Fade Out {Music}");
            
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                ForceComplete();
                _coroutine = null;
            }

            if (CurrentVolume == 0f)
                return;

            _coroutine = StartCoroutine(FadeTrack(StemData.FADE.OUT));
        }

        //Coroutines
        //====================================================================================================================//

        private IEnumerator FadeTrack(StemData.FADE fadeDirection)
        {
            var maxFadeTime = song.stems.Max(s => s.GetFadeTime(fadeDirection));
            var fadeData = song.stems.Select(x => x.GetFadeData(fadeDirection)).ToArray();

            float start;

            switch (fadeDirection)
            {
                case StemData.FADE.IN:
                    start = 0f;
                    _targetVolume = 1f;
                    break;
                case StemData.FADE.OUT:
                    start = 1f;
                    _targetVolume = 0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fadeDirection), fadeDirection, null);
            }

            var totalTime = 0f;
            while (totalTime <= maxFadeTime)
            {

                CurrentVolume = Mathf.Lerp(start, _targetVolume, totalTime / maxFadeTime);


                for (var i = 0; i < fadeData.Length; i++)
                {
                    FadeStem(ref song.stems[i], fadeData[i], totalTime);
                }


                totalTime += Time.unscaledDeltaTime;

                yield return null;
            }

            ForceComplete();

        }

        /*private static IEnumerator FadeStemCoroutine(StemData stemData, StemData.FADE fade)
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

        }*/

        //====================================================================================================================//

        private static void FadeStem(ref StemData stemData, in FadeData fadeData, in float totalTime)
        {
            if (fadeData.startDelay > 0 && totalTime < fadeData.startDelay)
                return;

            var timeIn = totalTime - fadeData.startDelay;

            var td = fadeData.curve.Evaluate(timeIn / fadeData.time);
            var vol = Mathf.Lerp(0f, stemData.maxLevel, td);

            stemData.SetVolume(vol);
        }

        private void ForceComplete()
        {
            CurrentVolume = _targetVolume;

            //Force set everyones volume, after to ensure that all are where they're meant to be
            for (var i = 0; i < song.stems.Length; i++)
            {
                song.stems[i].SetVolume(Mathf.Lerp(0f, song.stems[i].maxLevel, _targetVolume));
            }
        }

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
