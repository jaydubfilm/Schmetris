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
        
        attackTimer-=Time.deltaTime;
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
            if (neighbor.GetComponent<Brick>().IsCore()) {
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

}
