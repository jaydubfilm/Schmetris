using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Utilities;

namespace StarSalvager
{
    public class LevelManager : Singleton<LevelManager>
    {
        public static LevelManager Instance => m_instance;
        private static LevelManager m_instance;

        [SerializeField]
        private Bot m_botGameObject;
        public Bot BotGameObject => m_botGameObject;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.Log($"An instance of LevelManager already exists.");
            }

            m_instance = this;
        }
    }
}
