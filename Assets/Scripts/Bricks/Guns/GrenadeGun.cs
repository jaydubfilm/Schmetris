using UnityEngine;

//Crafted grenade launcher gun brick
public class GrenadeGun : Gun
{
    //Look for highest-priority enemy in range
    protected override GameObject FindTarget()
    {
        float closestDistance = float.MaxValue;
        int enemyPriority = int.MinValue;
        GameObject target = null;
        foreach (GameObject enemyObj in GameController.Instance.enemyList)
        {
            if (enemyObj && enemyObj.GetComponentInChildren<SpriteRenderer>().isVisible && enemyObj.GetComponent<Enemy>())
            {
                float dist = Vector3.Distance(enemyObj.transform.position, transform.position);
                if (dist <= range[parentBrick.GetPoweredLevel()])
                {
                    //Target the highest-priority enemy that is closest to the gun
                    if (enemyObj.GetComponent<Enemy>().strength > enemyPriority || (enemyObj.GetComponent<Enemy>().strength == enemyPriority && dist < closestDistance))
                    {

                        enemyPriority = enemyObj.GetComponent<Enemy>().strength;
                        closestDistance = dist;
                        target = enemyObj;
                    }
                }
            }
        }
        return target;
    }
}
