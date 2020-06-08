using UnityEngine;
using System.Collections.Generic;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]//Bomb brick that damages enemies
    public class Bomb : MonoBehaviour
    {
        //Damage effect
        public int[] damage;
        public int[] range;
        public GameObject bombEffect;

        //Damage all enemies within radius and spawn bomb effects
        public void BombEnemies(int level)
        {
            List<GameObject> enemyArr = new List<GameObject>();

            for (int x = 0; x < GameController.Instance.enemyList.Count; x++)
            {
                if (GameController.Instance.enemyList[x])
                {
                    if (Vector3.Distance(GameController.Instance.enemyList[x].transform.position, transform.position) <=
                        range[level])
                    {
                        enemyArr.Add(GameController.Instance.enemyList[x]);
                    }
                }
                else
                {
                    GameController.Instance.enemyList.RemoveAt(x--);
                }
            }

            for (int x = 0; x < enemyArr.Count; x++)
            {
                Brick brick = enemyArr[x].GetComponent<Brick>();
                if (brick != null)
                {
                    brick.AdjustHP(-damage[level]);
                    GameObject explosion = Instantiate(bombEffect, brick.transform.position, Quaternion.identity);
                }
                else
                {
                    Enemy enemy = enemyArr[x].GetComponent<Enemy>();
                    enemy.AdjustHP(-damage[level]);
                    GameObject explosion = Instantiate(bombEffect, enemy.transform.position, Quaternion.identity);
                }
            }
        }
    }
}