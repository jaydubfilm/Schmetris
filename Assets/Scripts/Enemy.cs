using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hP;
    public SpeciesData data;
    public Rigidbody2D rb2d;
    public GameObject targetBrick;
    LayerMask brickMask;
    Bot bot;

    // Start is called before the first frame update
    void Start()
    {
        hP = data.maxHP;
        GameController.Instance.enemyList.Add(gameObject);
        rb2d = GetComponent<Rigidbody2D>();
        //GetComponent<SpriteRenderer>().sprite = data.sprite;
        Vector3 dest = GameController.Instance.bot.transform.position;
        // rb2d.velocity = new Vector3(0,-data.speed,0);
        brickMask = LayerMask.GetMask("Brick");
        bot = GameController.Instance.bot;
        // targetBrick = bot.brickArr[0,0];
    }

    // Update is called once per frame
    void Update()
    {
        if (hP<=0) 
            DestroyEnemy();
   
        RaycastHit2D rH = Physics2D.Raycast(transform.position,bot.transform.position-transform.position, ScreenStuff.colSize,brickMask); 
        if (rH.collider!=null) {
            bot.ResolveEnemyCollision(gameObject);
        }
        MoveTowardsBot();
    }

    public void MoveTowardsBot(){
        float step = data.speed*Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, bot.transform.position, step);
    }   

    public void DestroyEnemy() {
        GameController.Instance.enemyList.Remove(gameObject);
        Destroy(gameObject);
    }
}

