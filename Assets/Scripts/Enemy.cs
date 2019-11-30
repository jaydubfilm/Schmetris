using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public int hP;
    public SpeciesData data;
    public Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        hP = data.maxHP;
        GameController.Instance.enemyList.Add(gameObject);
        rb2d = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().sprite = data.sprite;
        rb2d.velocity = new Vector3(0,-data.speed,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (hP<=0) 
            DestroyEnemy();

    }

    public void DestroyEnemy() {
        GameController.Instance.enemyList.Remove(gameObject);
        Destroy(gameObject);
    }
}

