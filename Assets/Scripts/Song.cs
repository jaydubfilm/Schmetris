using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Audio.ScriptableObjects;
using UnityEngine;

namespace StarSalvager.Audio
{
    public class Song : MonoBehaviour
    {
        [SerializeField, Required]
        private SongScriptableObject song;

        [Button]
        private void SetupObject()
        {
            throw new NotImplementedException();
        }
    }
}
