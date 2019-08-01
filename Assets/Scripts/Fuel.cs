using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fuel : MonoBehaviour
{
    public int fuelLevel;
    public int maxLevel;
    public int fuelDrop;
    public float fuelPeriod;
    private float fuelTimer;
    private GameObject marker;
    private bool emptyWarning;
    public int[] maxFuelArr;
    private int warningLevel = 20;

    public bool active;
    private List<GameObject> fuelBrickList; 
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
        fuelLevel = maxLevel;
        fuelBrickList = parentBrick.parentBot.GetComponent<Bot>().fuelBrickList;
        fuelBrickList.Add(gameObject);
        if (fuelBrickList.Count == 1)
            Activate();
    }

    void Update()
    {
        if (active) {
            if (Time.time >= fuelTimer + fuelPeriod) {
                fuelLevel -= fuelDrop;
                fuelTimer = Time.time;
            }   
        }
        if ((fuelLevel<= warningLevel) && (emptyWarning == false)){
            CancelInvoke();
            emptyWarning = true;
            InvokeRepeating("Blink",0,0.1f);
        }

        if (fuelLevel <= 0) {
            parentBrick.DestroyBrick();
        }
    }

    public void Activate()
    {
        active = true;
        marker.SetActive(true);
        fuelTimer = Time.time;  
        InvokeRepeating("Blink",0,0.5f);
    }

    public void Deactivate() {
        fuelBrickList.Remove(gameObject);
        CancelInvoke();
        active = false;
        marker.SetActive(false);
        if (fuelBrickList.Count != 0) {
            fuelBrickList[0].GetComponent<Fuel>().Activate();
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
