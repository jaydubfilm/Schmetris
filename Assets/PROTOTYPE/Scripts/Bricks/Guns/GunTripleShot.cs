using UnityEngine;
using System.Collections.Generic;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Multi-shot gun brick
    public class GunTripleShot : Gun
    {
        //Multi-shot targeting
        public int numberOfEnemies = 3;
        List<GameObject> targets = new List<GameObject>();

        //Find closest enemies in range to fire at - one shot per enemy
        protected override GameObject FindTarget()
        {
            targets = new List<GameObject>();

            foreach (GameObject enemyObj in GameController.Instance.enemyList)
            {
                if (enemyObj && !targets.Contains(enemyObj))
                {
                    float dist = Vector3.Distance(enemyObj.transform.position, transform.position);
                    if (dist <= range[parentBrick.GetPoweredLevel()])
                    {
                        if (targets.Count < numberOfEnemies)
                        {
                            targets.Add(enemyObj);
                        }
                        else
                        {
                            for (int i = 0; i < targets.Count; i++)
                            {
                                if (dist < Vector3.Distance(targets[i].transform.position, transform.position))
                                {
                                    targets.Insert(i, enemyObj);
                                    targets.RemoveAt(targets.Count - 1);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return targets.Count > 0 ? targets[0] : null;
        }

        //Shoot at targets, burn resources, and begin reload
        protected override void FireGun()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                GameObject newBulletObj = Instantiate(bullet[parentBrick.GetPoweredLevel()], transform.position,
                    Quaternion.identity);
                Bullet newBullet = newBulletObj.GetComponent<Bullet>();
                newBullet.SetAsHoming(targets[i].transform, targets[i].GetComponent<InvaderMovement>());
                newBullet.direction = Vector3.Normalize(targets[i].transform.position - transform.position);
                newBullet.speed = speed;
                newBullet.damage = attackPower[parentBrick.GetPoweredLevel()];
                newBullet.range = range[parentBrick.GetPoweredLevel()];
            }

            fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];
            GameController.Instance.bot.GetComponent<AudioSource>().PlayOneShot(fireSound, 0.5f);
        }
    }
}