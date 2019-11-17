using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bit : MonoBehaviour
{
    public int bitType;
    public int ID;
    public Vector2Int offset;
    public Vector2Int blockArrPos;
    public int bitLevel=0;

    GameObject parentObj;
    Block parentBlock;

    // Start is called before the first frame update

    void Awake ()
    {
        parentObj = transform.parent.gameObject;
        parentBlock = parentObj.GetComponent<Block>();
        FixedJoint2D fj = gameObject.GetComponent<FixedJoint2D>();
        fj.connectedBody = parentObj.GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        Vector3 parentOffsetV3 = parentObj.transform.position;
        offset = new Vector2Int (Mathf.RoundToInt((transform.position.x-parentOffsetV3.x) / ScreenStuff.colSize), Mathf.RoundToInt((transform.position.y-parentOffsetV3.y) / ScreenStuff.rowSize));
        blockArrPos = parentBlock.coreV2 - offset;
        parentBlock.bitList.Add(gameObject);
        parentBlock.bitArr[blockArrPos.x,blockArrPos.y] = gameObject;
        RotateUpright();
        ID = bitType*1000;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RemoveFromBlock(string actionType){

        if (transform.parent == null)
            return;

        gameObject.transform.parent = null;
        parentBlock.bitArr[blockArrPos.x,blockArrPos.y] = null;
        parentBlock.bitList.Remove(gameObject);

        if (parentBlock.bitList.Count==0)
            parentBlock.DestroyBlock();

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

    public bool CompareToBrick(Brick brick) {
        int compType = Mathf.RoundToInt((ID-bitLevel)/1000) - 2;

        return((compType == brick.brickType)&&(bitLevel==brick.brickLevel));
    }

}
