using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public static class GameTimer
    {
        public static bool IsPaused => m_paused;
        private static bool m_paused = false;

        private static List<IPausable> m_IPausables = new List<IPausable>();

        public static void SetPaused(bool value)
        {
            m_paused = value;

            foreach (IPausable pausable in m_IPausables)
            {
                if (m_paused)
                {
                    pausable.OnResume();
                }
                else
                {
                    pausable.OnPause();
                }
            }
        }

        public static void AddPausable(IPausable pausable)
        {
            m_IPausables.Add(pausable);
        }

        public static void RemovePausable(IPausable pausable)
        {
            m_IPausables.Remove(pausable);
        }
    }
}