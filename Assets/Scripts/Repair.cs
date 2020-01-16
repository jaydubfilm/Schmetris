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
            GameObject newHealSymbol = Instantiate(healingSymbol,gameObject.transform);
            
            StartCoroutine(MoveSymbol(newHealSymbol));
            StartCoroutine(FadeOutSymbol(newHealSymbol));

            targetBrick.AdjustHP(healPower[brick.brickLevel]);
            if (targetBrick.brickHP>=targetBrick.brickMaxHP[targetBrick.brickLevel]) {
                targetBrick.brickHP = targetBrick.brickMaxHP[targetBrick.brickLevel];
            }
        }
    }

    IEnumerator MoveSymbol (GameObject symbol) {
        Transform originalPos = gameObject.transform;
        Vector3 newPos = target.GetComponent<Rigidbody2D>().transform.position;
        float duration = (newPos-originalPos.position).magnitude/2.0f;
        float t = 0.0f;
        while (t< duration)
            {
                symbol.transform.position = Vector3.Lerp(originalPos.position,newPos,t/duration);
                yield return null;
                t+=Time.deltaTime;
            }
            symbol.transform.position = newPos;
    }

    IEnumerator FadeOutSymbol(GameObject symbol){
        SpriteRenderer spriteR = symbol.GetComponent<SpriteRenderer>();

        Color tmpColor = spriteR.color;
        float fadeTime = 1.0f;
        while (tmpColor.a > 0f) {
            tmpColor.a -= Time.deltaTime / fadeTime;
            spriteR.color = tmpColor;
            if (tmpColor.a <=0)
                tmpColor.a = 0;
            yield return null;
            spriteR.color = tmpColor;
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
