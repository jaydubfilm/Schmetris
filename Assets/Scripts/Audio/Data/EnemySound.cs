using System;
using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Audio.Data
{
     [Serializable]
    public struct EnemySound : IEquatable<EnemySound>
    {
        public static readonly EnemySound Empty = new EnemySound
        {
            enemyID = string.Empty,
            moveSound = LoopingSound.Empty,
            attackClip = null
        };
        
        
        [FoldoutGroup("$GetEnemyType"), ValueDropdown("GetEnemyTypes"), LabelText("Enemy")]
        public string enemyID;

        [FoldoutGroup("$GetEnemyType"), LabelWidth(80)]
        public LoopingSound moveSound;

        [FoldoutGroup("$GetEnemyType"), Required]
        public AudioClip attackClip;



        #region IEquatable

        public bool Equals(EnemySound other)
        {
            return enemyID == other.enemyID && moveSound.Equals(other.moveSound) &&
                   Equals(attackClip, other.attackClip);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemySound other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (enemyID != null ? enemyID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ moveSound.GetHashCode();
                hashCode = (hashCode * 397) ^ (attackClip != null ? attackClip.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion

        #region Unity Editor

#if UNITY_EDITOR
        private string GetEnemyType()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyName(enemyID);
        }

        private IEnumerable GetEnemyTypes()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyTypes();
        }
#endif

        #endregion


    }

}