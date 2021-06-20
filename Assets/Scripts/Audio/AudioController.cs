using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Audio.Data;
using StarSalvager.Audio.Enemies;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace StarSalvager.Audio
{
    public class AudioController : Singleton<AudioController>
    {
        public const string MASTER_VOLUME ="Master_Volume";
        public const string MUSIC_VOLUME ="Music_Volume";
        public const string SFX_VOLUME ="SFX_Volume";
        public const string SFX_PITCH ="SFX_Pitch";

        private const int MAX_CHANNELS = 2;

        private static float _sfxVolume = 1f;
        private static float _musicVolume = 1f;
        
        //Audio Sources
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Audio Sources")]
        private AudioSource sfxAudioSource;

        //Audio Mixers
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Audio Mixers")]
        private AudioMixer masterMixer;

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
        [SerializeField]
        private MUSIC startingMusic;
        
        [SerializeField]
        private List<Song> Songs;
        
        //Enemy Sounds
        //============================================================================================================//

        [FoldoutGroup("Enemy Sounds")]
        public FlySounds FlySounds;
        [FoldoutGroup("Enemy Sounds")]
        public DataLeechSounds DataLeechSounds;
        [FoldoutGroup("Enemy Sounds")]
        public MoonMinerSounds MoonMinerSounds;
        [FoldoutGroup("Enemy Sounds")]
        public VoltSounds VoltSounds;
        [FoldoutGroup("Enemy Sounds")]
        public SquartSounds SquartSounds;
        [FoldoutGroup("Enemy Sounds")]
        public ToughMotherSounds ToughMotherSounds;
        [FoldoutGroup("Enemy Sounds")]
        public ShardSounds ShardSounds;
        [FoldoutGroup("Enemy Sounds")]
        public SleeperMineSounds SleeperMineSounds;
        [FoldoutGroup("Enemy Sounds")]
        public SensorMineSounds SensorMineSounds;
        [FoldoutGroup("Enemy Sounds")]
        public BorrowerSounds BorrowerSounds;
        [FoldoutGroup("Enemy Sounds")]
        public LaserTurretSounds LaserTurretSounds;
        [FoldoutGroup("Enemy Sounds")]
        public PulseCannonSounds PulseCannonSounds;
        [FoldoutGroup("Enemy Sounds")]
        public IceWingSounds IceWingSounds;

        //====================================================================================================================//
        
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

        #region Static Functions

        //Static functions
        //============================================================================================================//

        public static void PlayBitConnectSound(BIT_TYPE bitType, float pitch = 1f)
        {
            SOUND sound;
            switch (bitType)
            {
                case BIT_TYPE.WHITE:
                case BIT_TYPE.BLUE:
                    sound = SOUND.BIT_SNAP_BLUE;
                    break;
                case BIT_TYPE.GREEN:
                    sound = SOUND.BIT_SNAP_GREEN;
                    break;
                case BIT_TYPE.GREY:
                    sound = SOUND.BIT_SNAP_GREY;
                    break;
                case BIT_TYPE.RED:
                    sound = SOUND.BIT_SNAP_RED;
                    break;
                case BIT_TYPE.YELLOW:
                    sound = SOUND.BIT_SNAP_YELLOW;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bitType), bitType, null);
            }

            PlaySound(sound, pitch);
        }

        #region Play Sounds

        /// <summary>
        /// Volume should be any value between 0.0 - 1.0. Pitch should be between 0.01 - 3.0
        /// </summary>
        public static void PlaySound(SOUND sound, float pitch = 1f)
        {
            if (Instance == null)
                return;

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
                Instance.PlayOneShot(sound);
        }
        public static void PlaySound(in BaseSound baseSound)
        {
            if (Instance == null)
                return;
            
            Instance.PlayOneShot(baseSound);
        }

        /// <summary>
        /// Volume should be any value between 0.0 - 1.0. Pitch should be between 0.01 - 3.0
        /// </summary>
        /// <param name="delayTime"></param>
        public static void PlayDelayedSound(SOUND sound, float delayTime)
        {
            if (Instance == null)
                return;

            Instance.StartCoroutine(DelayedEvent(delayTime, () =>
            {
                Instance.PlayOneShot(sound);
            }));
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
        
        //Loops
        //====================================================================================================================//

        public static void PlayLoop(in LoopingSound loopingSound)
        {
            if (Instance == null)
                return;

            Instance.PlayLoopingSound(loopingSound, out var _);
        }
        public static void PlayLoop(in LoopingSound loopingSound, out AudioSource audioSource)
        {
            audioSource = null;
            
            if (Instance == null)
                return;

            Instance.PlayLoopingSound(loopingSound, out audioSource);
        }
        
        public static void StopLoop(in LoopingSound loopingSound)
        {
            if (Instance == null)
                return;
            
            Instance.StopLoopingSound(loopingSound);
        }

        #endregion //Play Sounds

        #region Volume

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
            
            _sfxVolume = Mathf.Clamp01(volume);
            Instance.SetVolume(SFX_VOLUME, _sfxVolume);
        }

        #endregion //Volume

        #region Music

        /// <summary>
        /// Volume should be any value between 0.0 - 1.0
        /// </summary>
        /// <param name="volume"></param>
        public static void SetMusicVolume(float volume)
        {
            if (Instance == null)
                return;
            
            _musicVolume = Mathf.Clamp01(volume);
            Instance.SetVolume(MUSIC_VOLUME, _musicVolume);
        }

        public static void CrossFadeTrack(MUSIC trackTarget)
        {
            if (Instance == null)
                return;
            
            Instance.CrossFadeMusic(trackTarget);
        }

        #endregion //Music

        //============================================================================================================//

        [Obsolete]
        public static void PlayEnemyFireSound(string enemyId, float volume)
        {
            throw new NotImplementedException();
            /*if (string.IsNullOrEmpty(enemyId)) return;
            
            Instance?.EnemyFireSound(enemyId, volume);*/
        }
        [Obsolete]
        public static void PlayEnemyMoveSound(string enemyId)
        {
            throw new NotImplementedException();
            /*if (string.IsNullOrEmpty(enemyId)) return;
            
            Instance?.EnemyMoveSound(enemyId);*/
        }
        [Obsolete]
        public static void StopEnemyMoveSound(string enemyId)
        {
            throw new NotImplementedException();
            /*if (string.IsNullOrEmpty(enemyId)) return;
            
            Instance?.StopMoveSound(enemyId);*/
        }
        //============================================================================================================//

        #endregion //Static Functions
        

        //============================================================================================================//


        #region Instance Functions

        //Unity Function
        //====================================================================================================================//

        private void Start()
        {
            foreach (var song in Songs)
            {
                song.Mute();
            }
            
            CrossFadeMusic(startingMusic);

            var musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME, 1f);
            var sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME, 1f);
            
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
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

            PlayLoopingSound(enemySoundData.moveSound, out var _);
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
        

        //SFX Functions
        //============================================================================================================//

        private void PlayOneShot(in BaseSound baseSound)
        {
            if (baseSound == null) return;

            PlayOneShot(baseSound.clip, baseSound.Volume);
        }
        
        private void PlayOneShot(SOUND sound)
        {
            if (!TryGetSound(sound, out var soundClip))
                return;

            PlayOneShot(soundClip.clip, soundClip.Volume);
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

        private MUSIC _playingMusic = MUSIC.NONE;
        /*private MUSIC _playingMusic = MUSIC.NONE;
        private MUSIC _previousMusic = MUSIC.NONE;*/

        private void CrossFadeMusic(MUSIC target)
        {
            if (target == _playingMusic)
                return;
            
            /*FadeOutMusic(_playingMusic);
            FadeInMusic(target);*/

            foreach (var song in Songs)
            {
                if(song.Music == target)
                    song.FadeInTrack();
                else
                    song.FadeOutTrack();
            }
            //_previousMusic = _playingMusic;
            _playingMusic = target;
        }

        /*private void FadeInMusic(MUSIC music)
        {
            if (music == MUSIC.NONE)
                return;

            foreach (var song in Songs)
            {
                if(song.Music == music)
                    song.FadeInTrack();
                else
                    song.FadeOutTrack();
            }
            
            //Songs.FirstOrDefault(m => m.Music == music)?.FadeInTrack();
        }*/

        /*private void FadeOutMusic(MUSIC music)
        {
            if (music == MUSIC.NONE)
                return;
            
            Songs.FirstOrDefault(m => m.Music == music)?.FadeOutTrack();
        }*/
        
        
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
            }, out var _);
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
        
        private void PlayLoopingSound(in LoopingSound loopingSound, out AudioSource audioSource)
        {
            audioSource = null;
            
            if(activeLoopingSounds == null)
                activeLoopingSounds = new Dictionary<LoopingSound, Stack<AudioSource>>();
            
            if (!activeLoopingSounds.ContainsKey(loopingSound))
            {
                activeLoopingSounds.Add(loopingSound, new Stack<AudioSource>());   
            }

            if (activeLoopingSounds[loopingSound].Count >= loopingSound.maxChannels)
                return;

            //TODO Need to get use the recycling system here
            if (!Recycler.TryGrab(out AudioSource newAudioSource))
            {
                newAudioSource = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
            }
            
            newAudioSource.transform.SetParent(transform, false);
            newAudioSource.transform.localPosition = Vector3.zero;
            
            
            activeLoopingSounds[loopingSound].Push(newAudioSource);
            newAudioSource.outputAudioMixerGroup = sfxAudioMixerGroup;
            newAudioSource.clip = loopingSound.clip;
            newAudioSource.volume = loopingSound.volume;
            newAudioSource.loop = true;
            newAudioSource.Play();

            audioSource = newAudioSource;
        }

        private void StopLoopingSound(in LoopingSound loopingSound)
        {
            if (activeLoopingSounds.IsNullOrEmpty()) return;
            
            
            if (!activeLoopingSounds.ContainsKey(loopingSound)) return;
            
            //TODO Need to check if there are any existing sounds when trying to remove it

            if (activeLoopingSounds[loopingSound].Count <= 0) return;
            
            var audioSource = activeLoopingSounds[loopingSound].Pop();
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.volume = 1f;
            
            Recycler.Recycle<AudioSource>(audioSource);
        }
        
        //Volume Functions
        //============================================================================================================//
        
        private void SetVolume(string parameterName, float volume)
        {
            volume = Mathf.Clamp(volume, 0.001f, 1f);
            
            masterMixer.SetFloat(parameterName, Mathf.Log(volume) * 13);

            //Save the setting
            PlayerPrefs.SetFloat(parameterName, volume);
        }
        
        private float InverseVolume(float volume)
        {
            //volume = Mathf.Clamp(volume, 0.001f, 1f);

            volume = Mathf.Exp(volume / 13);
            volume = Mathf.Clamp(volume, 0.001f, 1f);

            //Mathf.Log(volume) * 13

            //masterMixer.SetFloat(parameterName, );

            return volume;
        }
        
        //============================================================================================================//

        private bool TryGetSoundClip(SOUND sound, out AudioClip clip)
        {
            return TryGetClip(soundClips, sound, out clip);
        }
        private bool TryGetSound(SOUND sound, out SoundClip baseSound)
        {
            return TryGetSound(soundClips, sound, out baseSound);
        }

        //============================================================================================================//
        
        private static bool TryGetClip<T>(IEnumerable<BaseSound<T>> list, T sound, out AudioClip clip) where T : Enum
        {
            clip = list.FirstOrDefault(s => Equals(s.sound, sound))?.clip;
            return clip != null;
        }
        
        private static bool TryGetSound<T>(IEnumerable<BaseSound<T>> list, T sound, out SoundClip baseSound) where T : Enum
        {
            baseSound = list.FirstOrDefault(s => Equals(s.sound, sound)) as SoundClip;
            return baseSound != null;
        }

        #endregion

        //============================================================================================================//

        private static IEnumerator DelayedEvent(float time, Action onFinishedCallback)
        {
            yield return new WaitForSeconds(time);
            
            onFinishedCallback?.Invoke();
        }

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
            /*var music = Enum.GetValues(typeof(MUSIC));*/

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
            
            /*//TODO Check to make sure all music exists in list
            foreach (MUSIC m in music)
            {
                if (musicClips.Any(s => s.sound == m))
                    continue;
                
                musicClips.Add(new MusicClip
                {
                    sound = m,
                    clip = null
                });
            }  */
            
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


