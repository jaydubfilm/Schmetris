using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game", menuName = "Game")]
public class Game : ScriptableObject
{
    public float storeCostMultiplier = 1.0f;
    public LevelData[] levelDataArr;
    //public bool schmetris;
    //public Bot startingBot;
}
