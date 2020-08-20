using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Audio.Data
{
    [Serializable]
    public class SoundClip : BaseSound<SOUND>
    {
        [ShowInInspector,TableColumnWidth(60), DisplayAsString,  PropertyOrder(-100)]
        public override SOUND sound { get; set; }
        
        public override AudioClip clip
        {
            get => _clip; 
            set => _clip = value; 
        }
        [SerializeField, AssetSelector(Paths = "Assets/Audio/SFX")]
        private AudioClip _clip;
    }
    
}
