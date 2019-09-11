using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bit : MonoBehaviour
{
    public int bitType;
    public Vector2Int arrPos;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void RemoveFromBlock(string actionType){

        if (transform.parent == null)
            return;

        Block parentBlock = transform.parent.gameObject.GetComponent<Block>();
        gameObject.transform.parent = null;
        parentBlock.bitArr[arrPos.x,arrPos.y] = null;

        switch (actionType) {
            case ("bounce"):
                Vector2 force = new Vector2 (Random.Range(-20,20),0);
                Rigidbody2D rb2D = gameObject.AddComponent<Rigidbody2D>();

                rb2D.AddForce(force,ForceMode2D.Impulse);
                rb2D.AddTorque(Random.Range(-10,10),ForceMode2D.Impulse);
                rb2D.gravityScale=1;
                //gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                
                gameObject.tag = "Moveable";
                break;
            case ("explode"):
                ExplodeBit();
                break;
            default :
                Destroy(gameObject);
                break;
        }
    }  

    public void ExplodeBit() {
        Animator anim;
        float animDuration;

        anim = gameObject.GetComponent<Animator>();
        anim.enabled = true;

        animDuration = 0.3f;
        StartCoroutine(DestroyAfterAnimation(animDuration));
    }

    IEnumerator DestroyAfterAnimation(float duration){  
        yield return new WaitForSeconds(duration);
        Destroy(gameObject); 
    }
}
