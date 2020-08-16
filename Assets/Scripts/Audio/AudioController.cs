using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Audio.Data;
using StarSalvager.Utilities;
using UnityEngine;
using UnityEngine.Audio;

namespace StarSalvager.Audio
{
    public class AudioController : Singleton<AudioController>
    {
        private const string MASTER_VOLUME ="Master_Volume";
        private const string MUSIC_VOLUME ="Music_Volume";
        private const string SFX_VOLUME ="SFX_Volume";
        private const string SFX_PITCH ="SFX_Pitch";
            
        
        //Audio Sources
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Audio Sources")]
        private AudioSource sfxAudioSource;
        //[SerializeField, Required, FoldoutGroup("Audio Sources")]
        //private AudioSource sfxAudioSourcePitched;
        [SerializeField, Required, FoldoutGroup("Audio Sources")]
        private AudioSource musicAudioSource;
        
        //Audio Mixers
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Audio Mixers")]
        private AudioMixer masterMixer;
        
        //Sound Lists
        //============================================================================================================//
        
        [SerializeField, BoxGroup("Sound Effects")]
        [TableList(DrawScrollView = true, MaxScrollViewHeight = 300, AlwaysExpanded = true, HideToolbar = true)]
        private List<SoundClip> soundClips;
        
        [SerializeField, BoxGroup("Music")]
        [TableList(DrawScrollView = true, MaxScrollViewHeight = 300, AlwaysExpanded = true, HideToolbar = true)]
        private List<MusicClip> musicClips;
        
        
        //Static functions
        //============================================================================================================//

        /// <summary>
        /// Volume should be any value between 0.0 - 1.0. Pitch should be between 0.01 - 3.0
        /// </summary>
        /// <param name="volume"></param>
        public static void PlaySound(SOUND sound, float volume = 1f, float pitch = 1f)
        {
            if (Instance == null)
                return;

            volume = Mathf.Clamp01(volume);
            pitch = Mathf.Clamp(pitch, 0.01f, 3f);
            
            if(pitch != 1f)
                Instance.PlaySoundPitched(sound, pitch);
            else
                Instance.PlayOneShot(sound, volume);
        }
        
        public static void PlayMusic(MUSIC music)
        {
            if (Instance == null)
                return;
            
            Instance.PlayMusicLoop(music);
        }

        /// <summary>
        /// Volume should be any value between 0.0 - 1.0
        /// </summary>
        /// <param name="volume"></param>
        public static void SetVolume(float volume)
        {
            if (Instance == null)
                return;
            
            volume = Mathf.Clamp01(volume);
            Instance.SetVolume(MASTER_VOLUME, volume);
            
        }
        /// <summary>
        /// Volume should be any value between 0.0 - 1.0
        /// </summary>
        /// <param name="volume"></param>
        public static void SetSFXVolume(float volume)
        {
            if (Instance == null)
                return;
            
            volume = Mathf.Clamp01(volume);
            Instance.SetVolume(SFX_VOLUME, volume);
        }
        /// <summary>
        /// Volume should be any value between 0.0 - 1.0
        /// </summary>
        /// <param name="volume"></param>
        public static void SetMusicVolume(float volume)
        {
            if (Instance == null)
                return;
            
            volume = Mathf.Clamp01(volume);
            Instance.SetVolume(MUSIC_VOLUME, volume);
        }

        //SFX Functions
        //============================================================================================================//

        private void PlayOneShot(SOUND sound, float volume)
        {
            var clip = soundClips.FirstOrDefault(s => s.sound == sound)?.clip;

            if (clip == null)
                return;
            
            sfxAudioSource.PlayOneShot(clip, volume);
        }

        private void PlaySoundPitched(SOUND sound, float pitch)
        {
            var clip = soundClips.FirstOrDefault(s => s.sound == sound)?.clip;

            if (clip == null)
                return;

            masterMixer.SetFloat(SFX_PITCH, pitch * 100f);
            
            //TODO Set Pitch here
            sfxAudioSource.clip = clip;
            sfxAudioSource.Play();
        }
        
        //Music Functions
        //============================================================================================================//

        private void PlayMusicLoop(MUSIC music)
        {
            //TODO Still need to setup fading here
            var clip = musicClips.FirstOrDefault(s => s.sound == music)?.clip;
            
            if (clip == null)
                return;

            //TODO Set Pitch here
            musicAudioSource.clip = clip;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
        
        //Volume Functions
        //============================================================================================================//
        
        private void SetVolume(string parameterName, float volume)
        {
            var vol = Mathf.Lerp(-80f, 0f, volume);    
            
            masterMixer.SetFloat(parameterName, vol);
        }
        
        //============================================================================================================//
        //============================================================================================================//
        //============================================================================================================//
        
        #region Auto-populate List
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            var sfx = Enum.GetValues(typeof(SOUND));
            var music = Enum.GetValues(typeof(MUSIC));

            //TODO Check to make sure all sounds exists in list
            foreach (SOUND sound in sfx)
            {
                if (soundClips.Any(s => s.sound == sound))
                    continue;
                
                soundClips.Add(new SoundClip
                {
                    sound = sound,
                    clip = null
                });
            }            
            
            //TODO Check to make sure all music exists in list
            foreach (MUSIC m in music)
            {
                if (musicClips.Any(s => s.sound == m))
                    continue;
                
                musicClips.Add(new MusicClip
                {
                    sound = m,
                    clip = null
                });
            }    
        }

        #endif
        
        #endregion //Auto-populate List
        
        //============================================================================================================//

    }
}


