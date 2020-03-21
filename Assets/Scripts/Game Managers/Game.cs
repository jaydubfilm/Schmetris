using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game", menuName = "Game")]
public class Game : ScriptableObject
{
    public LevelData[] levelDataArr;
    //public bool schmetris;
    //public Bot startingBot;
}
