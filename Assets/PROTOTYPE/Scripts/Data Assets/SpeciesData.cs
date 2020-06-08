using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Controls stats for enemy types
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
    public class SpeciesData : ScriptableObject
    {
        //Integer used by bot to identify the type of enemy and differentiate brick-enemies like parasites from player bricks
        public int type;

        //Enemy stats
        [Header("Stats")] public int maxHP;
        public float attackRate;
        public int damage;
        public float speed;
        public int dangerLevel; //Some guns will prioritize enemies with a higher danger level

        //Resources the player gains when the enemy is destroyed
        [Header("Resources")] public int redYield = 0;
        public int blueYield = 0;
        public int yellowYield = 0;
        public int greenYield = 0;
        public int greyYield = 0;
    }
}