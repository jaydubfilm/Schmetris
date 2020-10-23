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
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace StarSalvager.Audio
{
    public class AudioController : Singleton<AudioController>
    {
        private const string MASTER_VOLUME ="Master_Volume";
        private const string MUSIC_VOLUME ="Music_Volume";
        private const string SFX_VOLUME ="SFX_Volume";
        private const string SFX_PITCH ="SFX_Pitch";

        private const int MAX_CHANNELS = 2;
        
        //Audio Sources
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Audio Sources")]
        private AudioSource sfxAudioSource;
        //[SerializeField, Required, FoldoutGroup("Audio Sources")]
        //private AudioSource sfxAudioSourcePitched;
        [FormerlySerializedAs("musicAudioSource")] [SerializeField, Required, FoldoutGroup("Audio Sources")]
        private AudioSource menuMusicAudioSource;
        [SerializeField, Required, FoldoutGroup("Audio Sources")]
        private AudioSource gameMusicAudioSource;
        [SerializeField, Required, FoldoutGroup("Audio Sources")]
        private AudioSource scrapMusicAudioSource;
        
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

        //Music Properties
        //============================================================================================================//
        
        [FormerlySerializedAs("musicSnapshots")] 
        [SerializeField, FoldoutGroup("Music")]
        private AudioMixerSnapshot[] musicMixerSnapshots;

        [SerializeField, FoldoutGroup("Music")]
        private float musicFadeTime;
        
        [SerializeField, FoldoutGroup("Music")]
        private AudioClip[] TEST_waveMusic;
        
        [SerializeField, FoldoutGroup("Music"), PropertySpace(SpaceBefore = 10f)]
        [TableList(DrawScrollView = true, MaxScrollViewHeight = 300, AlwaysExpanded = true, HideToolbar = true)]
        private List<MusicClip> musicClips;
        
        //============================================================================================================//

        [SerializeField, PropertySpace(SpaceBefore = 10f)]
        [DetailedInfoBox("EnemyEffects AttackClip Only Plays as OneShot","This sound does not use the looping system, and is not affected by the max channel value")]
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


            switch (sound)
            {
                case SOUND.REPAIRER_PULSE:
                case SOUND.SHIELD_RECHARGE:
                    Instance.PlayLoopingSound(sound);
                    return;
            }
            
            
            if(pitch != 1f)
                Instance.PlaySoundPitched(sound, pitch);
            else
                Instance.PlayOneShot(sound, volume);
        }
        
        public static void StopSound(SOUND sound)
        {
            if (Instance == null)
                return;
            
            switch (sound)
            {
                case SOUND.REPAIRER_PULSE:
                case SOUND.SHIELD_RECHARGE:
                    Instance.StopLoopingSound(sound);
                    return;
            }
        }
        
        public static void PlayMusic(MUSIC music)
        {
            if (Instance == null)
                return;
            
            Instance.PlayMusicLoop(music);
        }

        public static void PlayMusic(MUSIC music, bool forceChange)
        {
            if (Instance == null)
                return;

            Instance.PlaySpecificMusic(music, forceChange);
        }

        public static void PlayTESTWaveMusic(int index, bool forceChange = false)
        {
            if (Instance == null)
                return;

            Instance.PlayWaveMusic(index, forceChange);
        }

        /// <summary>
        /// Volume should be any value between 0.0 - 1.0
        /// </summary>
        /// <param name="volume"></param>
        public static void SetVolume(float volume)
        {
            if (Instance == null)
                return;
            
            volume = Mathf.Clamp(volume, 0.001f, 1f);
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
            var enemySoundData = EnemyEffects.FirstOrDefault(x => x.enemyID == enemyId);

            if (enemySoundData.Equals(default))
            {
                return;
            }

            PlayLoopingSound(enemySoundData.moveSound);
        }

        private void StopMoveSound(string enemyId)
        {
            var enemySoundData = EnemyEffects.FirstOrDefault(x => x.enemyID == enemyId);
            
            if (enemySoundData.Equals(default))
            {
                return;
            }

            StopLoopingSound(enemySoundData.moveSound);
        }
        
        //============================================================================================================//

        
        
        //============================================================================================================//

        #region Instance Functions


        

        //SFX Functions
        //============================================================================================================//

        private void PlayOneShot(SOUND sound, float volume)
        {
            if (!TryGetSoundClip(sound, out AudioClip clip))
                return;

            PlayOneShot(clip, volume);
        }

        private void PlaySoundPitched(SOUND sound, float pitch)
        {
            if (!TryGetSoundClip(sound, out AudioClip clip))
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
            if (!TryGetMusicClip(music, out AudioClip clip))
                return;

            var weights = new float[Enum.GetValues(typeof(MUSIC)).Length];
            
            for (int i = 0; i < musicMixerSnapshots.Length; i++)
            {
                weights[i] = i == (int) music ? 1f : 0f;
            }
            
            musicMixer.TransitionToSnapshots(musicMixerSnapshots, weights, musicFadeTime);
            

            //TODO Set Pitch here
            //musicAudioSource.clip = clip;
            //musicAudioSource.loop = true;
            //musicAudioSource.Play();
        }

        private void PlayWaveMusic(int index, bool forceChange)
        {
            void Play()
            {
                gameMusicAudioSource.Stop();
                gameMusicAudioSource.clip = TEST_waveMusic[index];
                gameMusicAudioSource.Play();
            }
            
            if (forceChange)
            {
                Play();
                return;
            }
            
            StartCoroutine(TEST_MusicFadeCoroutine(1f, Play));
        }
        
        private void PlaySpecificMusic(MUSIC music, bool forceChange)
        {
            if (!TryGetMusicClip(music, out AudioClip clip))
                return;
            
            void Play()
            {
                gameMusicAudioSource.Stop();
                gameMusicAudioSource.clip = clip;
                gameMusicAudioSource.Play();
            }
            
            if (forceChange)
            {
                Play();
                return;
            }
            
            StartCoroutine(TEST_MusicFadeCoroutine(1f, Play));
        }

        private IEnumerator TEST_MusicFadeCoroutine(float fadeTime, Action onMutedCallback)
        {
            var t = 0f;
            var startVolume = gameMusicAudioSource.volume;
            

            while (t / fadeTime < 1f)
            {
                gameMusicAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                t += Time.deltaTime;
                yield return null;
            }
            
            onMutedCallback?.Invoke();
            t = 0f;
            
            while (t / fadeTime < 1f)
            {
                gameMusicAudioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeTime);
                t += Time.deltaTime;
                yield return null;
            }
        }
        
        //Looping Sounds
        //============================================================================================================//

        private void PlayLoopingSound(SOUND sound)
        {
            if (!TryGetSoundClip(sound, out AudioClip clip))
                return;
            
            PlayLoopingSound(new LoopingSound
            {
                clip = clip,
                maxChannels = MAX_CHANNELS
            });
        }
        
        private void StopLoopingSound(SOUND sound)
        {
            if (!TryGetSoundClip(sound, out AudioClip clip))
                return;
            
            StopLoopingSound(new LoopingSound
            {
                clip = clip,
                maxChannels = MAX_CHANNELS
            });
        }
        
        private void PlayLoopingSound(LoopingSound loopingSound)
        {
            if(activeLoopingSounds == null)
                activeLoopingSounds = new Dictionary<LoopingSound, Stack<AudioSource>>();
            
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
            
            
            if (!activeLoopingSounds.ContainsKey(loopingSound))
                return;
            
            //TODO Need to check if there are any existing sounds when trying to remove it

            if (activeLoopingSounds[loopingSound].Count <= 0)
                return;
            
            var audioSource = activeLoopingSounds[loopingSound].Pop();
            audioSource.Stop();
            audioSource.clip = null;
            
            Recycler.Recycle<AudioSource>(audioSource);
        }
        
        //Volume Functions
        //============================================================================================================//
        
        private void SetVolume(string parameterName, float volume)
        {
            volume = Mathf.Clamp(volume, 0.001f, 1f);
            
            masterMixer.SetFloat(parameterName, Mathf.Log(volume) * 13);
        }
        
        //============================================================================================================//

        private bool TryGetMusicClip(MUSIC music, out AudioClip clip)
        {
            return TryGetClip(musicClips, music, out clip);
        }
        private bool TryGetSoundClip(SOUND sound, out AudioClip clip)
        {
            return TryGetClip(soundClips, sound, out clip);
        }

        //============================================================================================================//
        
        private static bool TryGetClip<T>(IEnumerable<BaseSound<T>> list, T sound, out AudioClip clip) where T : Enum
        {
            clip = list.FirstOrDefault(s => Equals(s.sound, sound))?.clip;
            return clip != null;
        }

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
            
            foreach (var (_, enemyID) in FindObjectOfType<FactoryManager>().EnemyProfile.GetAllEnemyNamesIds())
            {
                if (EnemyEffects.Any(x => x.enemyID == enemyID))
                    continue;
                
                EnemyEffects.Add(new EnemySound
                {
                    attackClip = null,
                    enemyID = enemyID,
                    moveSound = new LoopingSound
                    {
                        clip = null,
                        maxChannels = MAX_CHANNELS
                    }
                });
            }
        }

        
        #endregion //Auto-populate List

#endif

        //============================================================================================================//

    }

    


}


