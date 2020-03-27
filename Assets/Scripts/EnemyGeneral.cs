using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGeneral : MonoBehaviour
{

    public GameController gameController;

    [Tooltip("Used to sort enemies from least to most powerful. Used to determine targets when firing")]
    public int strength;

    public int hp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void EnemyDeath()
    {

        gameController.enemyList.Remove(gameObject);

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
    }
}
