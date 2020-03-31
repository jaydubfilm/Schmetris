using UnityEngine;

[CreateAssetMenu(fileName = "New Level Section", menuName = "Level Section")]
public class SectionData : ScriptableObject
{
   public float levelDuration;
   public BlockSpawnData[] blocks;
   public SpeciesSpawnData[] speciesSpawnData;
   public float blockSpawnRate;
   public float enemySpawnRate;
   public float blockSpeed;
}


[System.Serializable]
public class SpeciesSpawnData
{
    public GameObject species;
    public int probability;
}
