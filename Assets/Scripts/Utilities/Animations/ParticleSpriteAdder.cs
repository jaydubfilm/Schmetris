using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.Particles
{
    public class ParticleSpriteAdder : MonoBehaviour
    {
        public ParticleSystem particleSystem;

        public Sprite[] Sprites;


        [Button("Add Sprites")]
        private void AddAllSprites()
        {
            foreach (var sprite in Sprites)
            {
                particleSystem.textureSheetAnimation.AddSprite(sprite);
            }
        
        }
    }
}

