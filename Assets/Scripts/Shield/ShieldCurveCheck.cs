using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCurveCheck : MonoBehaviour
{
    Brick thisBrick;
    Brick neighborBrick;
    bool curveLeft;
    bool isTop;
    public bool innerPiece;
    BoxCollider2D collider;
    SpriteRenderer spriteRenderer;


    void OnEnable()
    {
        thisBrick = GetComponentInParent<Brick>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (innerPiece == true)
        {

            if (other.transform.GetComponent<HorizontalShieldCenterPiece>())
            {

                GetComponent<SpriteRenderer>().enabled = true;
            }

            if (other.transform.GetComponent<VerticalShieldPieceExt>())
            {

                if (spriteRenderer.enabled == true)
                {

                    other.GetComponent<SpriteRenderer>().enabled = false;
                }
            }

            if (other.transform.GetComponent<HorizontalShieldPiece>())
            {

                other.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        if (other.transform.GetComponent<HorizontalShieldPiece>())
        {

            if (innerPiece == false)
            {

                GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }
}
