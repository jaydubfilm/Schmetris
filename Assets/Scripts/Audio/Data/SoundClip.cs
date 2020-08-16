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
        
        [ShowInInspector, AssetSelector(Paths = "Assets/Audio/SFX")]
        public override AudioClip clip { get; set; }
    }
    
}
