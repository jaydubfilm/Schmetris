using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level")]
public class LevelData : ScriptableObject
{
   public float levelDuration;
   public SpawnData[] spawns;
   public Shape[] shapes;
   public float spawnRate;
}
