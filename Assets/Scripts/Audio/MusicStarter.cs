using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager.Audio
{
    public class MusicStarter : MonoBehaviour, IReset
    {
        [SerializeField]
        private MUSIC music;
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        public void Activate()
        {
            AudioController.PlayMusic(music);
        }

        public void Reset()
        {
        }
    }
}

