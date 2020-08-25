using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Audio.Data;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

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
        [SerializeField, Required, FoldoutGroup("Audio Mixers")]
        private AudioMixer musicMixer;
        
        //Sound Lists
        //============================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Audio Mixer Groups")]
        private AudioMixerGroup sfxAudioMixerGroup;
        
        //Sound Lists
        //============================================================================================================//
        
        [SerializeField, FoldoutGroup("Sound Effects")]
        [TableList(DrawScrollView = true, MaxScrollViewHeight = 300, AlwaysExpanded = true, HideToolbar = true)]
        private List<SoundClip> soundClips;
        
        [SerializeField, FoldoutGroup("Music"), PropertySpace(SpaceBefore = 10f)]
        [TableList(DrawScrollView = true, MaxScrollViewHeight = 300, AlwaysExpanded = true, HideToolbar = true)]
        private List<MusicClip> musicClips;

        [SerializeField, FoldoutGroup("Music/Music Fade", Order = -100)]
        private AudioMixerSnapshot[] musicSnapshots;

        [SerializeField, FoldoutGroup("Music/Music Fade")]
        //MAIN_MENU
        //GAMEPLAY
        //SCRAPYARD
        private float musicFadeTime;
        
        //============================================================================================================//

        [SerializeField]
        private List<EnemySound> EnemyEffects;

        [SerializeField]
        private GameObject audioSourcePrefab;

        private Dictionary<LoopingSound, Stack<AudioSource>> activeLoopingSounds;

        //============================================================================================================//

        private new Transform transform => _transform ? _transform : _transform = gameObject.transform;
        private Transform _transform;
        //============================================================================================================//

        #region Staic Functions

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
        
        #endregion //Static Functions
        
        //============================================================================================================//

        public static void PlayEnemyFireSound(string enemyId, float volume)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            
            Instance?.EnemyFireSound(enemyId, volume);
        }
        
        public static void PlayEnemyMoveSound(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            
            Instance?.EnemyMoveSound(enemyId);
        }
        
        public static void StopEnemyMoveSound(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            
            Instance?.StopMoveSound(enemyId);
        }
        
        //============================================================================================================//

        private void EnemyFireSound(string enemyId, float volume)
        {
            var clip = EnemyEffects.FirstOrDefault(x => x.enemyID == enemyId).attackClip;
            
            PlayOneShot(clip, volume);
        }

        //TODO I'll need to play with the volume based on its proximity to the center of the screen
        private void EnemyMoveSound(string enemyId)
        {
            if(activeEnemySounds == null)
                activeEnemySounds = new Dictionary<EnemySound, Stack<AudioSource>>();
            
            var enemySoundData = EnemyEffects.FirstOrDefault(x => x.enemyID == enemyId);

            if (!activeEnemySounds.ContainsKey(enemySoundData))
            {
                activeEnemySounds.Add(enemySoundData, new Stack<AudioSource>());   
            }

            if (activeEnemySounds[enemySoundData].Count >= enemySoundData.maxChannels)
                return;

            //TODO Need to get use the recycling system here
            if (!Recycler.TryGrab<AudioSource>(out AudioSource newAudioSource))
            {
                newAudioSource = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
            }
            
            newAudioSource.transform.SetParent(transform, false);
            newAudioSource.transform.localPosition = Vector3.zero;
            
            
            activeEnemySounds[enemySoundData].Push(newAudioSource);
            newAudioSource.outputAudioMixerGroup = sfxAudioMixerGroup;
            newAudioSource.clip = enemySoundData.moveClip;
            newAudioSource.loop = true;
            newAudioSource.Play();

        }

        private void StopMoveSound(string enemyId)
        {
            if (activeEnemySounds == null || activeEnemySounds.Count == 0)
                return;
            
            var enemySoundData = EnemyEffects.FirstOrDefault(x => x.enemyID == enemyId);
            
            if (!activeEnemySounds.ContainsKey(enemySoundData))
                return;
            
            //TODO Need to check if there are any existing sounds when trying to remove it

            var toRemove = activeEnemySounds[enemySoundData].Pop();
            toRemove.Stop();
            toRemove.clip = null;
            
            Recycler.Recycle<AudioSource>(toRemove);
            
        }
        
        //============================================================================================================//

        #region Instance Functions

        //Looping Sounds
        //============================================================================================================//

        private void PlayLoopingSound(LoopingSound loopingSound)
        {
            if(activeLoopingSounds == null)
                activeLoopingSounds = new Dictionary<LoopingSound, Stack<AudioSource>>();
            
            //var enemySoundData = EnemyEffects.FirstOrDefault(x => x.enemyID == enemyId);

            if (!activeLoopingSounds.ContainsKey(loopingSound))
            {
                activeLoopingSounds.Add(loopingSound, new Stack<AudioSource>());   
            }

            if (activeLoopingSounds[loopingSound].Count >= loopingSound.maxChannels)
                return;

            //TODO Need to get use the recycling system here
            if (!Recycler.TryGrab<AudioSource>(out AudioSource newAudioSource))
            {
                newAudioSource = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
            }
            
            newAudioSource.transform.SetParent(transform, false);
            newAudioSource.transform.localPosition = Vector3.zero;
            
            
            activeLoopingSounds[loopingSound].Push(newAudioSource);
            newAudioSource.outputAudioMixerGroup = sfxAudioMixerGroup;
            newAudioSource.clip = loopingSound.clip;
            newAudioSource.loop = true;
            newAudioSource.Play();
        }

        private void StopLoopingSound(LoopingSound loopingSound)
        {
            if (activeLoopingSounds == null || activeLoopingSounds.Count == 0)
                return;
            
            
            if (!activeLoopingSounds.ContainsKey(enemySoundData))
                return;
            
            //TODO Need to check if there are any existing sounds when trying to remove it

            var toRemove = activeEnemySounds[enemySoundData].Pop();
            toRemove.Stop();
            toRemove.clip = null;
            
            Recycler.Recycle<AudioSource>(toRemove);
        }
        

        //SFX Functions
        //============================================================================================================//

        private void PlayOneShot(SOUND sound, float volume)
        {
            var clip = soundClips.FirstOrDefault(s => s.sound == sound)?.clip;

            PlayOneShot(clip, volume);
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
        
        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null)
                return;
            
            sfxAudioSource.PlayOneShot(clip, volume);
        }
        
        //Music Functions
        //============================================================================================================//

        private void PlayMusicLoop(MUSIC music)
        {
            //TODO Still need to setup fading here
            var clip = musicClips.FirstOrDefault(s => s.sound == music)?.clip;
            
            if (clip == null)
                return;

            var weights = new float[Enum.GetValues(typeof(MUSIC)).Length];
            
            for (int i = 0; i < musicSnapshots.Length; i++)
            {
                weights[i] = i == (int) music ? 1f : 0f;
            }
            
            musicMixer.TransitionToSnapshots(musicSnapshots, weights, musicFadeTime);
            

            //TODO Set Pitch here
            //musicAudioSource.clip = clip;
            //musicAudioSource.loop = true;
            //musicAudioSource.Play();
        }
        
        //Volume Functions
        //============================================================================================================//
        
        private void SetVolume(string parameterName, float volume)
        {
            var vol = Mathf.Lerp(-80f, 0f, volume);    
            
            masterMixer.SetFloat(parameterName, vol);
        }
        
        //============================================================================================================//

        #endregion
        
        //============================================================================================================//

#if UNITY_EDITOR

        #region Auto-populate List
        
        [Button(ButtonSizes.Large), DisableInPlayMode, HorizontalGroup("Row1", Order =  -100)]
        private void FindSFXAssets()
        {
            const string FILE_START = "sfx_SS_";

            foreach (SOUND sound in Enum.GetValues(typeof(SOUND)))
            {
                var soundName = sound.ToString().ToLower();
                var results = UnityEditor.AssetDatabase.FindAssets($"{FILE_START}{soundName}");
                
                if(results.Length == 0 || results.Length > 1)
                    throw new FileLoadException($"Multiple files found with the name: {FILE_START}{soundName}");

                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(results[0]);
                var audioClip = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;

                var index = soundClips.FindIndex(0, soundClips.Count, x => x.sound == sound);

                if(index < 0)
                {
                    soundClips.Add(new SoundClip
                    {
                        sound = sound,
                        clip = audioClip
                    });
                    
                    continue;
                }
                
                soundClips[index].clip = audioClip;
            }
            
            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.AssetDatabase.SaveAssets();

            
            //UnityEditor.AssetDatabase.GUIDToAssetPath()
            //UnityEditor.AssetDatabase.LoadAssetAtPath()
        }

        [Button("Refresh Data", ButtonSizes.Large), HorizontalGroup("Row1", Order =  -100)]
        private void PopulateValues()
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

        
        #endregion //Auto-populate List

#endif

        //============================================================================================================//

    }

    [Serializable]
    public struct LoopingSound
    {
        [FoldoutGroup("$GetEnemyType")]
        public int maxChannels;
        
        
        [FoldoutGroup("$GetEnemyType")]
        public AudioClip clip;
    }

    [Serializable]
    public struct EnemySound
    {
        [FoldoutGroup("$GetEnemyType"), ValueDropdown("GetEnemyTypes"), LabelText("Enemy")]
        public string enemyID;

        public LoopingSound moveSound;
        
        [FoldoutGroup("$GetEnemyType")]
        [DetailedInfoBox("AttackClip Only Plays as OneShot","This sound does not use the looping system, and is not affected by the max channel value")]
        public AudioClip attackClip;

        #region Unity Editor

#if UNITY_EDITOR

        private string GetEnemyType()
        {
            string value = enemyID;
            ValueDropdownList<string> enemyTypes = new ValueDropdownList<string>();
            foreach (EnemyProfileData data in Object.FindObjectOfType<FactoryManager>().EnemyProfile.m_enemyProfileData)
            {
                enemyTypes.Add(data.EnemyType, data.EnemyTypeID);
            }
            return enemyTypes.Find(s => s.Value == value).Text;
        }
        
        private static IEnumerable GetEnemyTypes()
        {
            ValueDropdownList<string> enemyTypes = new ValueDropdownList<string>();
            foreach (EnemyProfileData data in Object.FindObjectOfType<FactoryManager>().EnemyProfile.m_enemyProfileData)
            {
                enemyTypes.Add(data.EnemyType, data.EnemyTypeID);
            }
            return enemyTypes;
        }
        
#endif

        #endregion
    }
}


