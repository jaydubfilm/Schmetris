﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Audio.Data
{
    [Serializable]
    public struct LoopingSound : IEquatable<LoopingSound>
    {
        //====================================================================================================================//
        
        public static readonly LoopingSound Empty = new LoopingSound
        {
            clip = null,
            maxChannels = 0,
            volume = 0f
        };

        //====================================================================================================================//
        
        [LabelWidth(50), Required]
        public AudioClip clip;

        [HorizontalGroup("Row1"), Range(0, 32), LabelWidth(100)]
        public int maxChannels;
        [HorizontalGroup("Row1"), Range(0f, 1f), LabelWidth(100)]
        public float volume;

        //====================================================================================================================//

        public void Play()
        {
            AudioController.PlayLoop(this);
        }
        public void Play(out AudioSource audioSource)
        {
            AudioController.PlayLoop(this, out audioSource);
        }
        public void Stop()
        {
            AudioController.StopLoop(this);
        }
        
        #region IEquatable

        public bool Equals(LoopingSound other)
        {
            return maxChannels == other.maxChannels && Equals(clip, other.clip);
        }

        public override bool Equals(object obj)
        {
            return obj is LoopingSound other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (maxChannels * 397) ^ (clip != null ? clip.GetHashCode() : 0);
            }
        }

        #endregion

        //====================================================================================================================//
    }
}
