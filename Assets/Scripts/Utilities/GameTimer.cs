using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public static class GameTimer
    {
        public static bool IsPaused => m_paused;
        private static bool m_paused = false;

        public static void SetPaused(bool value)
        {
            m_paused = value;
        }
    }
}