using System.Collections;
using UnityEngine;

//Healing brick
public class Repair : MonoBehaviour
{
    //Healing stats
    public int[] healPower;
    public float[] healRange;
    public float[] healRate;

    //Components
    Bot bot;
    Brick brick;

    //Heal effect
    float healTimer;
    public GameObject healingSymbol;

    //Resources
    public int[] maxResource;

    //Init
    void Start()
    {
        bot = GameController.Instance.bot;
        brick = GetComponent<Brick>();
        healTimer = healRate[brick.GetPoweredLevel()];
    }

    // Update is called once per frame
    void Update()
    {
        if (healTimer <= 0)
        {
            TryHeal();
        }
        else
        {
            healTimer -= Time.deltaTime;
        }
    }

    //Check for something to heal and available resources
    void TryHeal()
    {
        GameObject target = FindNewTarget();
        if (target && brick.TryBurnResources(healPower[brick.GetPoweredLevel()]))
        {
            HealTarget(target.GetComponent<Brick>());
        }
    }

    //Find closest damaged brick in range
    public GameObject FindNewTarget()
    {
        float closestDistance = 99;
        GameObject newTarget = null;

        foreach (GameObject brickObj in bot.brickList)
        {
            Brick brick = brickObj.GetComponent<Brick>();
            if (!brick.IsParasite())
            {
                if (brick.brickHP < brick.brickMaxHP[brick.GetPoweredLevel()])
                {
                    float dist = Vector3.Distance(brickObj.transform.position, transform.position);
                    if ((dist < closestDistance) && (dist < healRange[brick.GetPoweredLevel()]))
                    {
                        closestDistance = dist;
                        newTarget = brickObj;
                    }
                }
            }
        }
        return newTarget;
    }

    //Heal damaged brick, consume resources, and restart timer
    public void HealTarget(Brick targetBrick)
    {
        targetBrick.AdjustHP(healPower[brick.GetPoweredLevel()]);
        if (targetBrick.brickHP >= targetBrick.brickMaxHP[targetBrick.GetPoweredLevel()])
        {
            targetBrick.brickHP = targetBrick.brickMaxHP[targetBrick.GetPoweredLevel()];
        }

        GameObject newHealSymbol = Instantiate(healingSymbol, gameObject.transform);
        StartCoroutine(MoveSymbol(newHealSymbol, targetBrick.transform));
        StartCoroutine(FadeOutSymbol(newHealSymbol));

        healTimer = healRate[brick.GetPoweredLevel()];
    }

    //Move healing effect toward brick
    IEnumerator MoveSymbol(GameObject symbol, Transform target)
    {
        Transform originalPos = gameObject.transform;
        Vector3 newPos = target.position;
        float duration = (newPos - originalPos.position).magnitude / 2.0f;
        float t = 0.0f;
        while (symbol && t < duration)
        {
            symbol.transform.position = Vector3.Lerp(originalPos.position, newPos, t / duration);
            yield return null;
            t += Time.deltaTime;
        }
    }

    //Fade healing effect over time
    IEnumerator FadeOutSymbol(GameObject symbol)
    {
        SpriteRenderer spriteR = symbol.GetComponent<SpriteRenderer>();

        Color tmpColor = spriteR.color;
        float fadeTime = 1.0f;
        while (tmpColor.a > 0f)
        {
            tmpColor.a -= Time.deltaTime / fadeTime;
            spriteR.color = tmpColor;
            if (tmpColor.a <= 0)
                tmpColor.a = 0;
            yield return null;
            spriteR.color = tmpColor;
        }

        Destroy(symbol);
    }
}
