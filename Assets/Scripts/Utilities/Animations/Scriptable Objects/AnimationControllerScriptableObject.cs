using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    [CreateAssetMenu(fileName = "Animation Controller", menuName = "Star Salvager/Scriptable Objects/Animation Controller")]
    public class AnimationControllerScriptableObject : ScriptableObject
    {
        [SerializeField]
        private AnimationState[] States;

        public AnimationScriptableObject GetDefaultAnimation()
        {
            if (States == null || States.Length < 1)
                return null;
            
            return States[0].Animation;
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

