using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGeneral : MonoBehaviour
{

    public GameController gameController;

    [Tooltip("Used to sort enemies from least to most powerful. Used to determine targets when firing")]
    public int strength;

    public int hp;
    public bool isParasite;
    int hpLastFrame;
    Enemy enemy;

    // Start is called before the first frame update
    void Start()
    {
        if (isParasite)
        {
            enemy = GetComponent<Enemy>();
            hp = enemy.hP;
            hpLastFrame = hp;
        }
    }

    private void Update()
    {
        if (isParasite)
        {
            if (hpLastFrame != hp)
            {
                enemy.hP = hp;
            }

            hpLastFrame = hp;
        }
    }

    private void OnEnable()
    {
        GameController.OnLevelComplete += OnLevelComplete;
    }

    private void OnDisable()
    {
        GameController.OnLevelComplete -= OnLevelComplete;
    }

    void OnLevelComplete()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector3(0, -GameController.Instance.blockSpeed * GameController.Instance.adjustedSpeed, 0);
    }

    // Update is called once per frame
    public void EnemyDeath()
    {

        gameController.enemyList.Remove(gameObject);

        if (GetComponent<MosquitoAI>())
        {
            GetComponent<MosquitoAI>().Death();
        }
        else if (GetComponent<Mama>())
        {
            GetComponent<Mama>().Death();
        }
        else if (GetComponent<InvaderMovement>())
        {
            GetComponent<InvaderMovement>().Death();
        }
    }
}
