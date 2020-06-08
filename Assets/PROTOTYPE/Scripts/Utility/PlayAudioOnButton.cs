using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Attach to buttons to play a sound effect when they're clicked
    public class PlayAudioOnButton : MonoBehaviour
    {
        //Components
        AudioSource audioSource;

        //Sound to play on click
        public AudioClip buttonAudio;
        public float volume = 0.5f;

        //Init
        private void Awake()
        {
            audioSource = Camera.main.GetComponent<AudioSource>();
            GetComponent<Button>().onClick.AddListener(() => { audioSource.PlayOneShot(buttonAudio, volume); });
        }
    }
}