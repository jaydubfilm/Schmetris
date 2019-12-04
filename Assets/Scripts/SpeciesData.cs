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
}
