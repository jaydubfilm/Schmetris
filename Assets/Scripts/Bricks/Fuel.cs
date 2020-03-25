using UnityEngine;

//Fuel resource - Reddite
public class Fuel : MonoBehaviour
{
    //Components
    Brick parentBrick;
    public GameObject flashingSymbol;

    //Available fuel based on brick and power level
    public int[] maxFuelArr;
    public float fuelLevel;
    bool lowFuelWarning = false;
    bool isBurningFuel = false;

    //Store change in brick's fuel level if power level changes
    public float fuelDiff = 0;

    //Init
    void Start()
    {
        parentBrick = GetComponent<Brick>();
        fuelLevel = maxFuelArr[parentBrick.GetPoweredLevel()];
        GameController.Instance.bot.fuelBrickList.Add(gameObject);
    }

    //Update available fuel if level increases
    public void UpgradeFuelLevel() {  
        fuelLevel = maxFuelArr[parentBrick.GetPoweredLevel()]; 
    }

    //Use fuel to power bot
    public void BurnFuel(float amount)
    {
        fuelLevel -= amount;
        if(fuelLevel <= 0)
        {
            parentBrick.DestroyBrick();
        }

        if (!isBurningFuel)
        {
            isBurningFuel = true;
            flashingSymbol.SetActive(true);
            InvokeRepeating("Blink", 0, 0.5f);
        }
        else
        {
            ToggleLowFuelWarning(GameController.Instance.bot.fuelBrickList.Count == 1);
        }
    }

    //Bot has returned to burning its own fuel
    public void CancelBurnFuel()
    {
        if(isBurningFuel)
        {
            isBurningFuel = false;
            CancelInvoke();
            flashingSymbol.SetActive(false);
        }
    }

    //Toggle fuel burn symbol on and off to indicate use
    void Blink()
    {
        flashingSymbol.SetActive(!flashingSymbol.activeSelf);
    }

    //Blink faster when low on fuel
    void ToggleLowFuelWarning(bool toggle)
    {
        if(toggle != lowFuelWarning)
        {
            lowFuelWarning = toggle;
            CancelInvoke();
            InvokeRepeating("Blink", 0, lowFuelWarning ? 0.1f : 0.5f);
        }
    }
}
