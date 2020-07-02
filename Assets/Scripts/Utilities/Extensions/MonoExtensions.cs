using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class MonoExtensions
    {
        public static void DelayedCall(this MonoBehaviour monoBehaviour, float seconds, Action DoCallback)
        {
            monoBehaviour.StartCoroutine(DoInCoroutine(seconds, DoCallback));
        }
        
        private static IEnumerator DoInCoroutine(float seconds, Action DoCallback)
        {
            yield return new WaitForSeconds(seconds);
            
            DoCallback?.Invoke();
        }
    }

}
