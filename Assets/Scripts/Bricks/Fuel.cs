using UnityEngine;

//Fuel resource - Reddite
public class Fuel : MonoBehaviour
{
    //Components
    Brick parentBrick;

    //Available fuel based on brick and power level
    public int[] maxFuelArr;
    public int fuelLevel;

    //Store change in brick's fuel level if power level changes
    public int fuelDiff = 0;

    //Init
    void Start()
    {
        parentBrick = GetComponent<Brick>();
        fuelLevel = maxFuelArr[parentBrick.GetPoweredLevel()];
    }

    //Update available fuel if level increases
    public void UpgradeFuelLevel() {  
        fuelLevel = maxFuelArr[parentBrick.GetPoweredLevel()]; 
    }
}
