using UnityEngine;

//Base enemy class
public class Enemy : MonoBehaviour
{
    //Enemy stats stored in external asset
    public SpeciesData data;

    //Enemy stats adjusted during gameplay
    public int hp;
    bool hasScored = false;
    bool isDestroyed = false;
    public int strength;

    //Components
    Rigidbody2D rb2d;
    public HealthBar healthBar;
    public AudioClip deathSound;

    //Init
    void Start()
    {
        Init();
    }

    //Inherited initialization
    protected virtual void Init()
    {
        hp = data.maxHP;
        strength = data.dangerLevel;
        rb2d = GetComponent<Rigidbody2D>();
        GameController.Instance.enemyList.Add(gameObject);
        healthBar.gameObject.SetActive(false);
    }

    //Listen for events only enabled
    private void OnEnable()
    {
        GameController.OnGameOver += DestroyEnemyOnGameEvent;
        GameController.OnLoseLife += DestroyEnemyOnGameEvent;
        GameController.OnLevelComplete += OnLevelComplete;
    }

    //Stop listening for events when inactive
    private void OnDisable()
    {
        GameController.OnGameOver -= DestroyEnemyOnGameEvent;
        GameController.OnLoseLife -= DestroyEnemyOnGameEvent;
        GameController.OnLevelComplete -= OnLevelComplete;
    }

    //Remove from targetable enemies when destroyed
    private void OnDestroy()
    {
        if (GameController.Instance.enemyList.Contains(gameObject))
        {
            GameController.Instance.enemyList.Remove(gameObject);
        }
    }

    //Update movement only if level isn't complete and enemy isn't destroyed
    private void Update()
    {
        if(!isDestroyed && !GameController.Instance.isLevelCompleteQueued)
        {
            UpdateMovement();
        }
    }

    //Enemy movement behaviour
    protected virtual void UpdateMovement()
    {

    }

    //Update enemy health bar and check for death when health is changed
    public void AdjustHP(int adjust)
    {
        hp = Mathf.Min(data.maxHP, hp + adjust);
        if (hp <= 0)
        {
            OnEnemyDeath();
        }
        else
        {
            if(hp >= data.maxHP && !healthBar.gameObject.activeSelf)
            {
                healthBar.gameObject.SetActive(true);
            }
            else if (hp < data.maxHP && healthBar.gameObject.activeSelf)
            {
                healthBar.gameObject.SetActive(false);
            }
            float normalizedHealth = (float)hp / (float)data.maxHP;
            healthBar.SetSize(normalizedHealth);
        }
    }

    //Player has destroyed the enemy
    protected virtual void OnEnemyDeath()
    {
        if (GameController.Instance.enemyList.Contains(gameObject))
        {
            GameController.Instance.enemyList.Remove(gameObject);
        }
        isDestroyed = true;
        ScoreEnemy();
        Camera.main.GetComponent<AudioSource>().PlayOneShot(deathSound, 1.0f);
    }

    //Award player resources for destroying the enemy
    void ScoreEnemy()
    {
        if (hasScored)
            return;
        hasScored = true;

        if (data.redYield > 0)
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

    //Stop enemy action when level is finished
    void OnLevelComplete()
    {
        rb2d.velocity = new Vector3(0, -GameController.Instance.blockSpeed * GameController.Instance.adjustedSpeed, 0);
    }

    //A game function such as level completion has destroyed the enemy - don't award resources
    public void DestroyEnemyOnGameEvent()
    {
        Destroy(gameObject);
    }
}
