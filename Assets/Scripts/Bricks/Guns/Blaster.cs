using UnityEngine;

//Asteroid-targeting blaster gun
public class Blaster : Gun
{

    //Look for closest asteroid in range that the player hasn't passed
    protected override GameObject FindTarget()
    {
        float closestDistance = float.MaxValue;
        GameObject target = null;
        foreach (GameObject asteroid in GameController.Instance.blockList)
        {
            if (asteroid && asteroid.GetComponentInChildren<Asteroid>() && asteroid.transform.position.y > transform.position.y)
            {
                float dist = Vector3.Distance(asteroid.transform.position, transform.position);
                if ((dist < closestDistance) && (dist <= range[parentBrick.GetPoweredLevel()]))
                {
                    closestDistance = dist;
                    target = asteroid;
                }
            }
        }
        return target;
    }
}
