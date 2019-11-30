using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level")]
public class LevelData : ScriptableObject
{
   public float levelDuration;
   public BlockSpawnData[] blocks;
   public SpeciesSpawnData[] speciesSpawnData;
   public Shape[] shapes;
   public float blockSpawnRate;
   public float enemySpawnRate;
}


[System.Serializable]
public class SpeciesSpawnData
{
    public GameObject species;
    public int probability;
}
