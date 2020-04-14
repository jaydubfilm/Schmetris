using UnityEngine;

//Level Data asset - Each Game asset plays through a series of Level data assets
[CreateAssetMenu(fileName = "New Level", menuName = "Level")]
public class LevelData : ScriptableObject
{
    //Each Level is made up of a series of Sections with different spawn data
    public SectionData[] levelSections;
}
