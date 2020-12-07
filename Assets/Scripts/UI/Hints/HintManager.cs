using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Interfaces;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Hints
{
    public enum HINT
    {
        NONE,
        MAGNET,
        BONUS,
        GUN,
        FUEL,
        HOME
    }
    
    [RequireComponent(typeof(HighlightManager))]
    public class HintManager : MonoBehaviour
    {
        [SerializeField, Required]
        private HighlightManager HighlightManager;

        public void TryShowHint(HINT hint)
        {
            
        }
    }
}
