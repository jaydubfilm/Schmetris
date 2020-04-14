using UnityEngine;

//Overall app settings
[CreateAssetMenu(fileName = "New Game Settings", menuName = "GameSettings")]
public class GameSettings : ScriptableObject
{
    //Game function specifications
    public bool Schmetris; 
    public bool OrphanFall = true;

    //Cell size
    public float colSize = 1.4f;
    public float rowSize = 1.4f;

    //Game area size
    public int screenRadius = 20;
    public int rows = 20;
    public float topEdgeOfWorld = 40;
    public float bottomEdgeOfWorld = -20;

    //Background image size
    public Sprite bgSprite;
    public float bgHeight = 278;
    public float bgWidth = 388;
    public float bgZDepth = 400;
    public Vector3 bgScale = new Vector3(21, 21, 1);

    //Bot size
    public int maxBotRadius = 6;
    public int blockRadius = 3;

    //Speed settings
    public float bgScrollSpeed = 0.1f;
    public float ghostMoveSpeed = 30f;
    public float[] speedLevels = new float[] { 0.5f, 0.75f, 1.0f, 1.5f, 2.0f };
    public int defaultSpeedLevel = 2;

    //Market prices of base resources
    public MarketLevelData reddite;
    public MarketLevelData blueSalt;
    public MarketLevelData greenAlgae;
    public MarketLevelData yellectrons;
    public MarketLevelData greyscale;
}
