using UnityEngine;

//Bot overheat effect
public class Overheat : MonoBehaviour
{
    //Bot overheat stats
    public int maxHeatLevel = 3;
    public float coolDownDuration = 3.0f;

    //Current heat level
    private int heatLevel = 0;
    private float lastHitTime;

    //Components
    public Sprite[] heatSpriteArr;

    //Add events when overheat is active
    private void OnEnable()
    {
        GameController.OnGameRestart += ResetHeat;
        GameController.OnLevelRestart += ResetHeat;
    }

    //Stop listening for events when inactive
    private void OnDisable()
    {
        GameController.OnGameRestart -= ResetHeat;
        GameController.OnLevelRestart -= ResetHeat;
    }

    //Remove overheating in between levels
    void ResetHeat()
    {
        heatLevel = 0;
    }

    //Add heat when bricks are destroyed
    public void AddHeat()
    {
        heatLevel++;
        UpdateHeatSprite();
        if (heatLevel > maxHeatLevel)
        {
            GameController.Instance.EndGame("CORE OVERHEATED");
        }
        lastHitTime = Time.time;
    }

    //Increase heat sprite opacity as heat level increases
    void UpdateHeatSprite()
    {
        int rad = gameObject.GetComponent<Bot>().maxBotRadius;
        Color overlayColor;
        GameObject heatOverlay;
        GameObject coreBrick;

        float l;

        coreBrick = gameObject.GetComponent<Bot>().brickArr[rad, rad];

        if (coreBrick == null)
            return;

        heatOverlay = coreBrick.transform.Find("HeatOverlay").gameObject;
        overlayColor = heatOverlay.GetComponent<SpriteRenderer>().color;
        l = (float)heatLevel;
        overlayColor.a = l / maxHeatLevel;
        heatOverlay.GetComponent<SpriteRenderer>().color = overlayColor;
    }

    //Lower heat over time
    public void RemoveHeat()
    {
        heatLevel--;
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
