using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level")]
public class LevelData : ScriptableObject
{
   public float levelDuration;
   public BlockSpawnData[] blocks;
   public Shape[] shapes;
   public float blockSpawnRate;
}
