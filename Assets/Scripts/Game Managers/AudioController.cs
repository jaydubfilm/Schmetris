using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    AudioSource audioSource;

    public AudioClip menuMusic;
    public AudioClip gameMusic;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

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

    IEnumerator FadeMusic(float fadeTime, float volume)
    {
        float time = 0;
        float startVolume = audioSource.volume;
        while(time < fadeTime)
        {
            time += Time.unscaledDeltaTime;
            audioSource.volume = startVolume + (volume - startVolume) * time / fadeTime;
            yield return 0;
        }
    }
}
