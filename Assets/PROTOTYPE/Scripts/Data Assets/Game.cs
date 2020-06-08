using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Game data asset - controls settings unique to a specific series of levels
    [CreateAssetMenu(fileName = "New Game", menuName = "Game")]
    public class Game : ScriptableObject
    {
        //Multiplier applied to the base brick buy and sell costs in the market 
        public float storeCostMultiplier = 1.0f;

        //Set of levels to be played in this game set
        public LevelData[] levelDataArr;
    }
}