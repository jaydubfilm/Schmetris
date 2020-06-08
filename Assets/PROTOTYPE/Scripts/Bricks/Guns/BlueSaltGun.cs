using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Basic resource-yield gun
    public class BlueSaltGun : Gun
    {
        //Resources
        public int[] maxResource;

        //Look for closest enemy in range
        protected override GameObject FindTarget()
        {
            float closestDistance = float.MaxValue;
            GameObject target = null;
            foreach (GameObject enemyObj in GameController.Instance.enemyList)
            {
                if (enemyObj)
                {
                    float dist = Vector3.Distance(enemyObj.transform.position, transform.position);
                    if ((dist < closestDistance) && (dist <= range[parentBrick.GetPoweredLevel()]))
                    {
                        closestDistance = dist;
                        target = enemyObj;
                    }
                }
            }

            return target;
        }
    }
}