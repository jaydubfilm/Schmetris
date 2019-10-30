using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bit : MonoBehaviour
{
    public int bitType;
    public Vector2Int offset;

    GameObject parentBlock;

    // Start is called before the first frame update

    void Awake ()
    {
        parentBlock = transform.parent.gameObject;
        Vector3 parentOffsetV3 = parentBlock.transform.position;
    
        offset = new Vector2Int (Mathf.RoundToInt((transform.position.x-parentOffsetV3.x) / ScreenStuff.colSize), Mathf.RoundToInt((transform.position.y-parentOffsetV3.y) / ScreenStuff.rowSize));
    }
    
    void Start()
    {
        parentBlock.GetComponent<Block>().bitList.Add(gameObject);
        
        RotateUpright();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void RemoveFromBlock(string actionType){

        if (transform.parent == null)
            return;

        gameObject.transform.parent = null;
        // parentBlock.GetComponent<Block>().bitArr[arrPos.x,arrPos.y] = null;
        parentBlock.GetComponent<Block>().bitList.Remove(gameObject);

        switch (actionType) {
            case ("bounce"):
                ScreenStuff.BounceObject(gameObject);
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

    public void RotateUpright() {
        transform.rotation = Quaternion.identity;
    }

    IEnumerator DestroyAfterAnimation(float duration){  
        yield return new WaitForSeconds(duration);
        Destroy(gameObject); 
    }

    public int ConvertToBrickType(){
        return bitType - 2;
    }
}
