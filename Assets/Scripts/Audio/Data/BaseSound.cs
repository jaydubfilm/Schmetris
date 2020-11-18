using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Audio.Data
{
    [Serializable]
    public abstract class BaseSound<E> where E : Enum
    {
        public abstract E sound { get; set; }
        public abstract AudioClip clip { get; set; }

/*#if UNITY_EDITOR

        private string playButtonText => playingSound ? "Stop" : "Play";
        private bool playingSound;
        private AudioSource playingSource;
        private AudioClip previousClip;
        
        [TableColumnWidth(80, false)]
        [Button("$playButtonText"), DisableIf("HasNoSound")]
        private void Listen()
        {
            if (playingSound)
            {
                if(playingSource == null)
                    return;

                playingSource.clip = previousClip;
                playingSource.Stop();

            
            
                playingSource = null;
                previousClip = null;
            
                playingSound = false;
                return;
            }
            
            if(clip == null)
                return;
            
            playingSource = Object.FindObjectOfType<AudioSource>();
            previousClip = playingSource.clip;
            
            playingSource.Stop();
            playingSource.clip = clip;
            playingSource.Play();

            playingSound = true;
        }

        private bool HasNoSound()
        {
            return clip == null;
        }

#endif*/
    }
}