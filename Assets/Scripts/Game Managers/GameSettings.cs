﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Settings", menuName = "GameSettings")]
public class GameSettings : ScriptableObject
{
    public bool Schmetris; 
    public bool OrphanFall = true;
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
    public float[] speedLevels = new float[] { 0.5f, 0.75f, 1.0f, 1.5f, 2.0f };
    public int defaultSpeedLevel = 2;

    public MarketLevelData reddite;
    public MarketLevelData blueSalt;
    public MarketLevelData greenAlgae;
    public MarketLevelData yellectrons;
    public MarketLevelData greyscale;
}
