using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repair : MonoBehaviour
{
    public int[] healPower;
    public float[] healRange;
    public float[] healRate;
    Bot bot;
    Brick brick;
    GameObject target;
    float startTime;
    public GameObject healingSymbol;

    // Start is called before the first frame update
    void Start()
    {
        bot = GameController.Instance.bot;
        startTime = Time.time;
        brick = GetComponent<Brick>();
        // target = FindNewTarget();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime >= healRate[brick.brickLevel])
        {
            target = FindNewTarget();
            HealTarget();
            startTime = Time.time;
        }
    }

    public void HealTarget(){
        if (target!=null){
            Brick targetBrick = target.GetComponent<Brick>();
            GameObject newHealSymbol = Instantiate(healingSymbol,targetBrick.transform);
            StartCoroutine(WaitToEraseHealSymbol(newHealSymbol));

            targetBrick.AdjustHP(healPower[brick.brickLevel]);
            if (targetBrick.brickHP>=targetBrick.brickMaxHP[targetBrick.brickLevel]) {
                targetBrick.brickHP = targetBrick.brickMaxHP[targetBrick.brickLevel];
                // target = FindNewTarget();
            }
        }
    }

    IEnumerator WaitToEraseHealSymbol(GameObject symbol){
        yield return new WaitForSeconds(1.0f);
        Destroy(symbol);
    }

    public GameObject FindNewTarget(){
        float closestDistance = 99;
        GameObject newTarget = null;

        foreach (GameObject brickObj in bot.brickList){
            Brick brick = brickObj.GetComponent<Brick>();
            if (!brick.IsParasite()) {
                if (brick.brickHP<brick.brickMaxHP[brick.brickLevel]) {
                    float dist = Vector3.Distance(brickObj.transform.position,transform.position);
                    if ((dist<closestDistance) && (dist<healRange[brick.brickLevel])){
                        closestDistance = dist;
                        newTarget = brickObj;
                    }
                }
            }
        }
        return newTarget;
    }
}
