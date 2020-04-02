using UnityEngine;
using UnityEngine.UI;

public class PlayAudioOnButton : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip buttonAudio;

    private void Awake()
    {
        audioSource = Camera.main.GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(() => { audioSource.PlayOneShot(buttonAudio, 0.5f); });
    }
}
