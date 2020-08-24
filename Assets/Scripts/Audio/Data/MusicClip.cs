using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Audio.Data
{
    [Serializable]
    public class MusicClip : BaseSound<MUSIC>
    {
        public override MUSIC sound
        {
            get => Music;
            set => Music = value;
        }

        [SerializeField, TableColumnWidth(60), DisplayAsString, PropertyOrder(-100)]
        private MUSIC Music;

        public override AudioClip clip
        {
            get => _clip; 
            set => _clip = value; 
        }
        [SerializeField, AssetSelector(Paths = "Assets/Audio/Music")]
        private AudioClip _clip;
    }

}