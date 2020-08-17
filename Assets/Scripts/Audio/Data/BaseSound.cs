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

#if UNITY_EDITOR

        [TableColumnWidth(80, false)]
        [Button("Play"), DisableIf("HasNoSound")]
        private void PlaySound()
        {
            Object.FindObjectOfType<AudioSource>().PlayOneShot(clip, 1f);
        }

        private bool HasNoSound()
        {
            return clip == null;
        }

#endif
    }
}