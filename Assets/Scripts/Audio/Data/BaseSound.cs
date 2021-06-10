using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Audio.Data
{
    [Serializable]
    public abstract class BaseSound<E>: BaseSound where E : Enum
    {
        public abstract E sound { get; set; }
    }
    
    [Serializable]
    public abstract class BaseSound
    {
        public abstract AudioClip clip { get; set; }
        
        public float Volume
        {
            get => _volume; 
            set => _volume = value; 
        }
        [SerializeField, Range(0f,1f),TableColumnWidth(50)]
        private float _volume = 1f;
        
        public void Play()
        {
            AudioController.PlaySound(this);
        }
    }
}