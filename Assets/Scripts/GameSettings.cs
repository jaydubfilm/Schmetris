using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Settings", menuName = "GameSettings")]
public class GameSettings : ScriptableObject
{
    public bool Schmetris; 
    public float colSize = 1.4f;
    public float rowSize = 1.4f;
    public int screenRadius = 20;
    public int rows = 20;
    public float topEdgeOfWorld = 40;
    public float bottomEdgeOfWorld = -20;
    public Sprite bgSprite;
    public float bgHeight = 278;
    public float bgWidth = 388;
    public float bgZDepth = 400;
    public float bgScrollSpeed = 0.1f;
    public Vector3 bgScale = new Vector3(21,21,1);
    public int maxBotRadius = 6;
    public float ghostMoveSpeed = 30f;
    public int blockRadius = 3;
}
