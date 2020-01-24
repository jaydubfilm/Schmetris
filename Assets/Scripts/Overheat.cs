using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overheat : MonoBehaviour
{
    public int maxHeatLevel = 3;
    public float coolDownDuration = 3.0f;

    private int heatLevel = 0;
    private float lastHitTime;

    public Sprite[] heatSpriteArr;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    public void AddHeat() {
        heatLevel++;
        UpdateHeatSprite();
        if (heatLevel > maxHeatLevel)
            GameController.Instance.EndGame("CORE OVERHEATED - Game Over");
        lastHitTime = Time.time; 
    }

    void UpdateHeatSprite() {
        int rad = gameObject.GetComponent<Bot>().maxBotRadius;
        Color overlayColor;
        GameObject heatOverlay;
        GameObject coreBrick;

        float l;

        coreBrick = gameObject.GetComponent<Bot>().brickArr[rad,rad];

        if (coreBrick==null)
            return;

        heatOverlay = coreBrick.transform.Find("HeatOverlay").gameObject;
        overlayColor = heatOverlay.GetComponent<SpriteRenderer>().color;
        l = (float)heatLevel;
        overlayColor.a = l/maxHeatLevel;
        heatOverlay.GetComponent<SpriteRenderer>().color = overlayColor;
    }

    public void RemoveHeat() {
        heatLevel --;
        UpdateHeatSprite();
        lastHitTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if ((heatLevel > 0) && ((Time.time - lastHitTime) > coolDownDuration)) {
            RemoveHeat();
        }
    }
}
