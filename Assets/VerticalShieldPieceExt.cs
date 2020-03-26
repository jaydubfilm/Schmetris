using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalShieldPieceExt : MonoBehaviour
{

    public bool isEndPiece;
    public SpriteRenderer attachedSprite;
    SpriteRenderer spriteRenderer;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isEndPiece)
        {
            if (attachedSprite.enabled == false) 
            spriteRenderer.enabled = false;
        }
    }
}
