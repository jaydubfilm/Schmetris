using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    [DefaultExecutionOrder(-10000)]
    public class GameManager : MonoBehaviour
    {
        [SerializeField, Required]
        private GameSettingsScriptableObject m_gameSettings;

        void Awake()
        {
            m_gameSettings.SetupGameSettings();
        }
    }
}