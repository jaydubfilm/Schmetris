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

    //public SpriteRenderer ShieldFlatPiece2, ShieldFlatPiece1, ShieldFlatPiece0, ShieldFlatPiece_1, ShieldFlatPiece_2, Shield_Curve_Outer_UL, Shield_Curve_Outer_UR, Shield_Curve_Outer_DL, Shield_Curve_Outer_DR, Shield_Curve_Inner_UL, Shield_Curve_Inner_UR, Shield_Curve_Inner_DL, Shield_Curve_Inner_DR;

    void OnEnable()
    {
        thisBrick = GetComponentInParent<Brick>();

        GetComponent<SpriteRenderer>().enabled = false;
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

                other.GetComponent<SpriteRenderer>().enabled = false;
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
