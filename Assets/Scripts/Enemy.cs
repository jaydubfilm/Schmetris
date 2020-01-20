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

    private void OnEnable()
    {
        GameController.OnGameOver += DestroyEnemy;
    }

    private void OnDisable()
    {
        GameController.OnGameOver -= DestroyEnemy;
    }

    // Start is called before the first frame update
    void Start()
    {
        hP = data.maxHP;
        GameController.Instance.enemyList.Add(gameObject);
        rb2d = GetComponent<Rigidbody2D>();
        Vector3 dest = GameController.Instance.bot.transform.position;
        brickMask = LayerMask.GetMask("Brick");
        bot = GameController.Instance.bot;
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

