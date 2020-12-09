using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public class FrameStop : Singleton<FrameStop>
    {
        private bool stopped;
        
        //============================================================================================================//
        
        public static void Frames(int frames)
        {
            if (Instance == null) return;
            if(Instance.stopped) return;

            Instance.StartCoroutine(FrameStopCoroutine(frames));
        }
        
        
        public static void Seconds(float seconds)
        {
            if (Instance == null) return;
            
            if(Instance.stopped) return;

            Instance.StartCoroutine(FrameStopCoroutine(seconds));
        }
        public static void Milliseconds(int milliseconds)
        {
            if (Instance == null) return;
            
            if(Instance.stopped) return;

            Instance.StartCoroutine(FrameStopCoroutine(milliseconds / 1000f));
        }
        
        //============================================================================================================//

        private static IEnumerator FrameStopCoroutine(int frames)
        {
            var count = 0;

            Time.timeScale = 0;
            
            while (count++ < frames)
            {
                yield return null;
            }
            
            Time.timeScale = 1f;

        }
        
        private static IEnumerator FrameStopCoroutine(float time)
        {
            var t = 0f;

            Time.timeScale = 0;
            
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                
                yield return null;
            }
            
            Time.timeScale = 1f;

        }
        
        //============================================================================================================//
    }
}