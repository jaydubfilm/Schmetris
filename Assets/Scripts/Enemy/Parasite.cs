using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parasite : MonoBehaviour
{

    public SpeciesData data;
    public GameObject targetBrick;
    public Brick brick;
    public float attackTimer;
    public GameObject attackPrefab;

    //Resources upon destroying
    bool hasScored = false;
    public AudioClip deathSound;

    // Start is called before the first frame update
    void Start()
    {
        brick = gameObject.GetComponent<Brick>();
        attackTimer = 0;
        StartCoroutine(WaitAndSetAttackRate());
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0) {
            AttackTarget();
            attackTimer = data.attackRate;
        }   
    }

    IEnumerator WaitAndSetAttackRate()
    {
        yield return new WaitForSeconds(1.0f);
        attackTimer = data.attackRate;
    }

    public void AttackTarget() {
        if (targetBrick == null) 
            ChooseNewTarget();
        if (targetBrick != null)
        {
            targetBrick.GetComponent<Brick>().AdjustHP(-data.damage);
            GameObject attack = Instantiate(attackPrefab, transform.position, Quaternion.identity);
            attack.GetComponent<EnemyBullet>().Init(targetBrick.transform);
        }
    }

    public void ChooseNewTarget() {
        if (brick.neighborList.Count==0)
            return;
        foreach (GameObject neighbor in brick.neighborList) {
            if (neighbor && neighbor.GetComponent<Brick>().IsCore()) {
                targetBrick = neighbor;
                return;
            }
         }

        int targetInt = Random.Range(0,brick.neighborList.Count);
        targetBrick = brick.neighborList[targetInt];
        if (targetBrick.GetComponent<Brick>().IsParasite()) {
            targetBrick = null;
            return;
        }
    }

    public void ScoreEnemy()
    {
        if (hasScored)
            return;
        hasScored = true;

        Camera.main.GetComponent<AudioSource>().PlayOneShot(deathSound, 1.0f);

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
}
