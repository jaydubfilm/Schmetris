using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class SpeciesData : ScriptableObject
{
    public int type;
    public int maxHP;
    public float attackRate;
    public int damage;
    public float speed;

    //Resource yields on destruction
    public int redYield = 0;
    public int blueYield = 0;
    public int yellowYield = 0;
    public int greenYield = 0;
    public int greyYield = 0;
}
