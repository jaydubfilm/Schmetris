using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Values;
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


#if UNITY_EDITOR
        public void OnValidate()
        {
            m_gameSettings.SetupGameSettings();
        }
#endif

        public void Awake()
        {
            m_gameSettings.SetupGameSettings();
        }
    }
}