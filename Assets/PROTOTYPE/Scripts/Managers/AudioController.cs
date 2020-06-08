using System.Collections;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Controls background music throughout app
    public class AudioController : MonoBehaviour
    {
        //Components
        AudioSource audioSource;

        //Audio files
        public AudioClip menuMusic;
        public AudioClip gameMusic;

        //Init
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        //Stop old background music, fade new music in over time
        public void FadeInMusic(AudioClip clip, float startTime, float fadeTime)
        {
            if (clip != audioSource.clip)
            {
                audioSource.volume = 0;
                audioSource.clip = clip;
                audioSource.time = startTime;
                audioSource.Play();
                StopAllCoroutines();
                StartCoroutine(FadeMusic(fadeTime, 0.5f));
            }
        }

        //Fade audio source to target volume over time
        IEnumerator FadeMusic(float fadeTime, float volume)
        {
            float time = 0;
            float startVolume = audioSource.volume;
            while (time < fadeTime)
            {
                time += Time.unscaledDeltaTime;
                audioSource.volume = startVolume + (volume - startVolume) * time / fadeTime;
                yield return 0;
            }
        }
    }
}