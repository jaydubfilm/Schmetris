using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public class FrameStop : Singleton<FrameStop>
    {
        private bool stopped;
        
        //============================================================================================================//
        
        public void Frames(int frames)
        {
            if(stopped)
                return;

            StartCoroutine(FrameStopCoroutine(frames));
        }
        
        
        public void Seconds(float seconds)
        {
            if(stopped)
                return;

            StartCoroutine(FrameStopCoroutine(seconds));
        }
        public void Milliseconds(int milliseconds)
        {
            if(stopped)
                return;

            StartCoroutine(FrameStopCoroutine(milliseconds / 1000f));
        }
        
        //============================================================================================================//

        private IEnumerator FrameStopCoroutine(int frames)
        {
            var count = 0;

            Time.timeScale = 0;
            
            while (count++ < frames)
            {
                yield return null;
            }
            
            Time.timeScale = 1f;

        }
        
        private IEnumerator FrameStopCoroutine(float time)
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