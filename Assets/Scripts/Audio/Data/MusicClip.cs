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

        [ShowInInspector, AssetSelector(Paths = "Assets/Audio/Music")]
        public override AudioClip clip { get; set; }
    }

}