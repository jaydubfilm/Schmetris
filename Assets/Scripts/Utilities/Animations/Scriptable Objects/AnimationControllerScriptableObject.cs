using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    [CreateAssetMenu(fileName = "Animation Controller", menuName = "Star Salvager/Animations/Animation Controller")]
    public class AnimationControllerScriptableObject : ScriptableObject
    {
        private const string DEFAULT = "Default";
        
        [SerializeField]
        private AnimationState[] States = 
        {
            new AnimationState
            {
                StateName = DEFAULT,
                Animation = null
            }
        };

        public AnimationScriptableObject GetDefaultAnimation()
        {
            if (States == null || States.Length < 1)
                return null;
            
            return States.FirstOrDefault(x => x.StateName == DEFAULT).Animation;
        }

        public AnimationScriptableObject GetAnimation(string StateName)
        {
            var hashed = Animator.StringToHash(StateName);

            return GetAnimation(hashed);
        }
        
        public AnimationScriptableObject GetAnimation(int hashID)
        {
            return States.FirstOrDefault(x => x.HashedID == hashID).Animation;
        }
    }


}

