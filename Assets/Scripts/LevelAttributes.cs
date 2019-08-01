using UnityEngine;

public class LevelAttributes : MonoBehaviour
{
    public int[] eProbabilityArr = new int[GameController.bitCount];
    public float levelDuration;
/* 
    // Start is called before the first frame update
    void Awake()
    {
        GameController.eProbArr = gameObject.GetComponent<LevelAttributes>().eProbabilityArr;
        GameController.timeRemaining = levelDuration;
              
    }
    */
}
