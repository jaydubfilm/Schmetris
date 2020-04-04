using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ShieldBrick : MonoBehaviour
{
    Brick parentBrick;
    BoxCollider2D box2D;

    public int shieldHp;
    [Range(1, 5)]
    public int radius = 1;
    ShieldTrigger childTrigger;
    SpriteRenderer outlineSR;
    int hpAtStart;


    // Start is called before the first frame update
    void Start()
    {

        childTrigger = GetComponentInChildren<ShieldTrigger>();
        box2D = childTrigger.transform.GetComponent<BoxCollider2D>();
        hpAtStart = shieldHp;
        SetShieldSize();
        //outlineSR = childTrigger.transform.GetComponent<SpriteRenderer>();
    }

    [Button]
    public void SetShieldSize()
    {

        float newSize = ConvertRadius(radius);
        box2D.size = new Vector2(newSize, newSize);
        childTrigger.CheckRadius();
    }

    float ConvertRadius(int radius)
    {
       
        float convertedRadius = (ScreenStuff.colSize * 3) + (ScreenStuff.colSize * 2 * (radius - 1));
        return convertedRadius;
    }

    [Button]
    public void ReceiveDamage(int damage)
    {

        shieldHp += damage;
        foreach (Brick brick in childTrigger.protectedList)
        {
            if(brick != null)
            { 
                outlineSR = brick.transform.GetComponentInChildren<OutlineCheck>().transform.GetComponent<SpriteRenderer>();

                if (shieldHp < hpAtStart * 0.67f)
                    outlineSR.color = Color.yellow;
                if (shieldHp < hpAtStart * 0.35f)
                    outlineSR.color = Color.red;
            }
        }

        
    }

    public void DestroyShield()
    {
        //destroy shield
        if (shieldHp <= 0)
        {
            foreach (Brick protectedBrick in childTrigger.protectedList)
            {

                Destroy(protectedBrick.GetComponentInChildren<OutlineCheck>().gameObject);
                protectedBrick.activeShields.Remove(this);
                print("dead");
            }
        }
    }
}
