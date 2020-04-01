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
        GameController.OnLoseLife += DestroyEnemy;
        GameController.OnLevelComplete += OnLevelComplete;
    }

    private void OnDisable()
    {
        GameController.OnGameOver -= DestroyEnemy;
        GameController.OnLoseLife -= DestroyEnemy;
        GameController.OnLevelComplete -= OnLevelComplete;
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
        if (hP <= 0)
        {
            ScoreEnemy();
            DestroyEnemy();
        }
        else if (!GameController.Instance.isLevelCompleteQueued)
        {
            MoveTowardsBot();
        }
    }

    void OnLevelComplete()
    {
        rb2d.velocity = new Vector3(0, -GameController.Instance.blockSpeed * GameController.Instance.adjustedSpeed, 0);
    }

    public void MoveTowardsBot(){
        float step = data.speed*Time.deltaTime;

        //RaycastHit2D rH = Physics2D.Raycast(transform.position, bot.transform.position - transform.position, ScreenStuff.colSize, brickMask);
        RaycastHit2D rH = Physics2D.BoxCast(transform.position, Vector2.one * ScreenStuff.colSize, 0, bot.transform.position - transform.position, step, brickMask);
        if (rH.collider != null)
        {
            bot.ResolveEnemyCollision(gameObject);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, bot.transform.position, step);
        }
    }   

    public void ScoreEnemy()
    {
        if(data.redYield > 0)
        {
            GameController.Instance.bot.storedRed += data.redYield;
            GameController.Instance.CreateFloatingText(data.redYield.ToString(), transform.position + new Vector3(1, 1, 0), 30, Color.red);
        }
        if (data.blueYield > 0)
        {
            GameController.Instance.bot.storedBlue += data.blueYield;
            GameController.Instance.CreateFloatingText(data.blueYield.ToString(), transform.position + new Vector3(-1, 1, 0), 30, Color.blue);
        }
        if (data.yellowYield > 0)
        {
            GameController.Instance.bot.storedYellow += data.yellowYield;
            GameController.Instance.CreateFloatingText(data.yellowYield.ToString(), transform.position + new Vector3(1, -1, 0), 30, Color.yellow);
        }
        if (data.greenYield > 0)
        {
            GameController.Instance.bot.storedGreen += data.greenYield;
            GameController.Instance.CreateFloatingText(data.greenYield.ToString(), transform.position + new Vector3(-1, -1, 0), 30, Color.green);
        }
        if (data.greyYield > 0)
        {
            GameController.Instance.bot.storedGrey += data.greyYield;
            GameController.Instance.CreateFloatingText(data.greyYield.ToString(), transform.position, 30, Color.grey);
        }
    }

    public void DestroyEnemy() {
        GameController.Instance.enemyList.Remove(gameObject);
        Destroy(gameObject);
    }


}

