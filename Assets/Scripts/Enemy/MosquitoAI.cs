using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MosquitoAI : MonoBehaviour
{
    AIDestinationSetter aiDestinationSetter;
    AIPath aiPath;
    Transform player;
    GameObject pivot;
    SpringJoint2D spring;

    public float timer = 3;
    public float attackDistance = 50;
    bool attackMode;
    bool attached;
    public AnimationCurve enemyDistance;

    private void Start()
    {

        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        player = aiDestinationSetter.target;

        enemyDistance.preWrapMode = WrapMode.PingPong;
        enemyDistance.postWrapMode = WrapMode.PingPong;

        //pivot = new GameObject();
        //pivot.name = "name";
        //pivot.AddComponent<Rigidbody2D>().isKinematic = true;
        //pivot.AddComponent<SpringJoint>
    }

    private void Update()
    {

        if (attackMode == false)
        {

            if (Time.time > timer)
            {
                CheckDistance();
                timer = timer + Time.time;
            }
        }

        else
        {

            //attacking behaviour
            print("Attacking");
            aiPath.canMove = false;
            

            if(attached == false)
            {

                spring = gameObject.AddComponent<SpringJoint2D>();
                spring.connectedBody = player.GetComponent<Rigidbody2D>();
                spring.autoConfigureDistance = false;
                spring.enableCollision = true;
                attached = true;
                print("attached");
            }
        
                spring.distance = enemyDistance.Evaluate(Time.time);
        }
    }
    void CheckDistance()
    {

        print(Vector3.SqrMagnitude(transform.position - player.position));

        if (Vector3.SqrMagnitude(transform.position - player.position) < attackDistance)
        {

            attackMode = true;
        }
    }



}
