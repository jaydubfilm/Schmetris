using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fuel : MonoBehaviour
{
    private int fuelLevel;
    private int maxLevel;
    private int fuelDrop=1;
    public float fuelPeriod=1f;
    private float fuelTimer;
    private GameObject marker;
    private bool emptyWarning;
    public int[] maxFuelArr;
    private int warningLevel;
    
    public bool active;
    Bot bot;
    
    Brick parentBrick;


    // Start is called before the first frame update
    void Start()
    {
        active = false;
        emptyWarning = false;
        marker = transform.Find("FuelSymbol").gameObject;
        marker.SetActive(false);
        parentBrick = gameObject.GetComponent<Brick>();
        maxLevel = maxFuelArr[parentBrick.brickLevel];
        warningLevel = Mathf.Max(10,maxLevel/5);
        fuelLevel = maxLevel;
        bot = GameController.Instance.bot;
        bot.fuelBrickList.Add(gameObject);
        if (bot.fuelBrickList.Count == 1)
            Activate();
    }

    void Update()
    {
        if (active) {
            if (Time.time >= fuelTimer + fuelPeriod) {
                fuelLevel -= fuelDrop;
                if ((fuelLevel<= warningLevel) && (emptyWarning == false) && (bot.fuelBrickList.Count<=1)){
                    LowFuelWarning();
                }
                fuelTimer = Time.time;
            }   
        }

        if ((emptyWarning == true) && (bot.fuelBrickList.Count>1))
            CancelLowFuelWarning();
        
        if (fuelLevel <= 0) {
            parentBrick.DestroyBrick();
        }
    }

    public void LowFuelWarning(){
        CancelInvoke();
        emptyWarning = true;
        InvokeRepeating("Blink",0,0.1f);
    }

    public void CancelLowFuelWarning(){
        CancelInvoke();
        emptyWarning = false;
        InvokeRepeating("Blink",0,0.5f);
    }


    public void Activate()
    {
        active = true;
        marker.SetActive(true);
        fuelTimer = Time.time;  
        InvokeRepeating("Blink",0,0.5f);
    }

    public void Deactivate() {
        bot.fuelBrickList.Remove(gameObject);
        CancelInvoke();
        active = false;
        marker.SetActive(false);
        if (bot.fuelBrickList.Count != 0) {
            bot.fuelBrickList[0].GetComponent<Fuel>().Activate();
        }
    }

    public void Blink(){
        if (marker.activeSelf == true)
            marker.SetActive(false);
        else 
            marker.SetActive(true);
    }

    public void UpgradeFuelLevel() {
        int fuelLoss = maxLevel-fuelLevel;
        maxLevel = maxFuelArr[parentBrick.brickLevel];
        fuelLevel = maxLevel-fuelLoss;
    }
}
