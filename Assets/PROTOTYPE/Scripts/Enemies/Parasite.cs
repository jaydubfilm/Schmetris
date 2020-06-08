using System.Collections;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Parasite behaviour once attached as a Brick
    public class Parasite : Enemy
    {
        //Components
        public GameObject targetBrick;
        public Brick brick;

        //Attack effect
        public float attackTimer;
        public GameObject attackPrefab;

        //Parasites become higher priority once attached
        public int dangerLevelOverride;
        public int newHP;

        protected override void Init()
        {
            base.Init();
            hp = newHP;
            brick = gameObject.GetComponent<Brick>();
            attackTimer = 0;
            strength = dangerLevelOverride;
            brick.brickMaxHP[0] = data.maxHP;
            brick.brickHP = newHP;
            AdjustHP(0);
            StartCoroutine(WaitAndSetAttackRate());
        }

        //Attack attached brick
        protected override void UpdateLiveBehaviour()
        {
            base.UpdateLiveBehaviour();
            attackTimer -= Time.deltaTime;
            if (attackTimer < 0)
            {
                AttackTarget();
                attackTimer = data.attackRate;
            }
        }

        //Destroy brick component once scored
        protected override void OnEnemyDeath()
        {
            base.OnEnemyDeath();
            GetComponent<Brick>().ExplodeBrick();
        }

        //Wait a moment after attaching to start attacking
        IEnumerator WaitAndSetAttackRate()
        {
            yield return new WaitForSeconds(1.0f);
            attackTimer = data.attackRate;
        }

        //Deal damage to brick
        public void AttackTarget()
        {
            if (targetBrick == null)
                ChooseNewTarget();
            if (targetBrick != null)
            {
                targetBrick.GetComponent<Brick>().AdjustHP(-data.damage);
                GameObject attack = Instantiate(attackPrefab, transform.position, Quaternion.identity);
                attack.GetComponent<EnemyBullet>().Init(targetBrick.transform);
            }
        }

        //Find a new brick if not attached to anything
        public void ChooseNewTarget()
        {
            if (brick.neighborList.Count == 0)
                return;
            foreach (GameObject neighbor in brick.neighborList)
            {
                if (neighbor && neighbor.GetComponent<Brick>().IsCore())
                {
                    targetBrick = neighbor;
                    return;
                }
            }

            int targetInt = Random.Range(0, brick.neighborList.Count);
            targetBrick = brick.neighborList[targetInt];
            if (targetBrick.GetComponent<Brick>().IsParasite())
            {
                targetBrick = null;
                return;
            }
        }

        //Separate Parasite from Bot at end of level
        protected override void OnLevelComplete()
        {
            GetComponent<Brick>().MakeOrphan();
        }

        //Make sure this element is properly removed from the Bot before destroying it
        public override void DestroyEnemyOnGameEvent()
        {
            GetComponent<Brick>().DestroyBrick();
        }
    }
}