using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Damage Profile", menuName = "Star Salvager/Scriptable Objects/Damage Profile")]
    public class DamageProfileScriptableObject : ScriptableObject
    {
        [SerializeField, DisableInPlayMode]
        private List<Sprite> damageDetailSprites;
        [SerializeField, DisableInPlayMode]
        private List<Sprite> damageMaskSprites;

        private int count;
        
        //============================================================================================================//

        public Sprite GetDetailSprite(float value)
        {
            var index = GetIndex(value);
            return index < 0 ? null : damageDetailSprites[index];
        }
        
        public Sprite GetMaskSprite(float value)
        {
            var index = GetIndex(value);
            return index < 0 ? null : damageDetailSprites[index];
        }

        //============================================================================================================//

        private void CountCheck()
        {
            var check = damageDetailSprites.Count == damageMaskSprites.Count;

            if (!check) throw new Exception("Detail & Mask counts do not match");
            
            count = damageDetailSprites.Count;
        }

        private int GetIndex(float value)
        {
            CountCheck();

            var indexFloat = 1f / count;
            var index = -1;

            var val = 1f - value;
            for (var i = 1; i <= count; i++)
            {
                if (val > (indexFloat * i))
                    continue;
                
                
                index = i - 1;
                break;
            }

            if (index >= count) return -1;
            

            return index;
        }
        
        //============================================================================================================//
        
    }
}


