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
        [SerializeField, Required]
        private SongScriptableObject song;

        private bool _fading;
        
        [Button]
        public void SetFullVolume()
        {
            foreach (var stem in song.stems)
            {
                stem.SetVolume(1f);
            }
        }
        [Button]
        public void Mute()
        {
            foreach (var stem in song.stems)
            {
                stem.SetVolume(0f);
            }
        }

        //====================================================================================================================//
        public void FadeInTrack()
        {
            if (_fading)
                return;

            StartCoroutine(FadeTrack(StemData.FADE.IN));
        }

        public void FadeOutTrack()
        {
            if (_fading)
                return;

            StartCoroutine(FadeTrack(StemData.FADE.OUT));
        }

        //====================================================================================================================//
        
        private  IEnumerator FadeTrack(StemData.FADE fadeDirection)
        {
            _fading = true;
            
            var maxFadeTime = song.stems.Max(s => s.GetFadeTime(fadeDirection));
            var stems = song.stems.Select(stem => stem.FadeStem(fadeDirection)).ToArray();
            var coroutines = new Coroutine[stems.Length];

            for (int i = 0; i < stems.Length; i++)
            {
                coroutines[i] = StartCoroutine(stems[i]);
            }

            yield return new WaitForSecondsRealtime(maxFadeTime);
            _fading = false;
        }

        //====================================================================================================================//
        
        [Button]
        private void SetupObject()
        {
            throw new NotImplementedException();
        }

        //====================================================================================================================//
        
    }
}
