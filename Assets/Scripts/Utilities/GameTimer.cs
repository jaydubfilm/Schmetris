using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace StarSalvager.Utilities
{
    public static class GameTimer
    {
        public static bool IsPaused => m_paused;
        private static bool m_paused;

        private static List<IPausable> m_IPausables = new List<IPausable>();


        public static TimeSpan GetTimePlayed => DateTime.Now - startingTime;
        private static DateTime startingTime = DateTime.Now;

        public static void SetPaused(bool value)
        {
            m_paused = value;

            foreach (IPausable pausable in m_IPausables)
            {
                if (m_paused)
                {
                    pausable.OnPause();
                }
                else
                {
                    pausable.OnResume();
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