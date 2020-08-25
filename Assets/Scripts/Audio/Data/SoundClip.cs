using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Audio.Data
{
    [Serializable]
    public class SoundClip : BaseSound<SOUND>
    {
        public override SOUND sound
        {
            get => _sound;
            set => _sound = value;
        }
        [SerializeField, TableColumnWidth(60), DisplayAsString,  PropertyOrder(-100)]
        private SOUND _sound;

        public override AudioClip clip
        {
            get => _clip; 
            set => _clip = value; 
        }
        [SerializeField, AssetSelector(Paths = "Assets/Audio/SFX")]
        private AudioClip _clip;
    }
    
}
