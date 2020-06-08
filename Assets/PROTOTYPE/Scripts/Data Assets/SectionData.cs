using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Section data used by Levels to control duration and spawn rates
    [CreateAssetMenu(fileName = "New Level Section", menuName = "Level Section")]
    public class SectionData : ScriptableObject
    {
        //How long this Section will last - set to 0 for an untimed section that is ended by other means
        public float levelDuration;

        //How fast should blocks move in this section?
        public float blockSpeed;

        //How often should objects in this section spawn?
        public float blockSpawnRate;
        public float enemySpawnRate;

        //Types of objects to spawn in this section
        public BlockSpawnData[] blocks;
        public SpeciesSpawnData[] speciesSpawnData;
    }

//Controls the spawn probability of different types of blocks
    [System.Obsolete("Prototype Only Script")]
    [System.Serializable]
    public class BlockSpawnData
    {
        public GameObject block;
        public int probability;
    }

//Controls the spawn probability of different types of enemies
    [System.Obsolete("Prototype Only Script")]
    [System.Serializable]
    public class SpeciesSpawnData
    {
        public GameObject species;
        public int probability;
    }
}