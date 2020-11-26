using UnityEngine;
using UnityEngine.Audio;

namespace StarSalvager.Utilities.Extensions
{
    public static class AudioMixerExtensions
    {
        public static void SetVolume(this AudioMixer audioMixer, string parameter, float volume)
        {
            volume = Mathf.Clamp(volume, 0.001f, 1f);
            
            audioMixer.SetFloat(parameter, Mathf.Log(volume) * 13);
        }
        
        public static float GetNormalizeVolume(this AudioMixer audioMixer, string parameter)
        {
            if (!audioMixer.GetFloat(parameter, out var volume))
                return default;
            
            //volume = Mathf.Clamp(volume, 0.001f, 1f);

            volume = Mathf.Exp(volume / 13);
            volume = Mathf.Clamp(volume, 0.001f, 1f);

            //Mathf.Log(volume) * 13

            //masterMixer.SetFloat(parameterName, );

            return volume;
        }
    }
}
