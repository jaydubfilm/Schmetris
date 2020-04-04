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
    int maxHP;
    public HealthBar healthBar;

    public AudioClip deathSound;

    public void AdjustHP(int adjust)
    {
        hp += adjust;
        if(hp <= 0)
        {
            EnemyDeath();
        }
        else
        {
            if (hp >= maxHP)
            {
                hp = maxHP;
                healthBar.gameObject.SetActive(false);
            }
            else if (!healthBar.gameObject.activeSelf)
            {
                healthBar.gameObject.SetActive(true);
                float normalizedHealth = (float)hp / (float)maxHP;
                healthBar.SetSize(normalizedHealth);
            }
            else
            {
                float normalizedHealth = (float)hp / (float)maxHP;
                healthBar.SetSize(normalizedHealth);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isParasite)
        {
            enemy = GetComponent<Enemy>();
            hp = enemy.hP;
            hpLastFrame = hp;
        }
        maxHP = hp;
        healthBar.gameObject.SetActive(false);
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

    private void OnDestroy()
    {
        if(gameController.enemyList.Contains(gameObject))
        {
            gameController.enemyList.Remove(gameObject);
        }
    }

    // Update is called once per frame
    public void EnemyDeath()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(deathSound, 1.0f);

        if (gameController.enemyList.Contains(gameObject))
        {
            gameController.enemyList.Remove(gameObject);
        }

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
        else if (GetComponent<Enemy>())
        {
            GetComponent<Enemy>().ScoreEnemy();
            GetComponent<Enemy>().DestroyEnemy();
        }
    }
}
