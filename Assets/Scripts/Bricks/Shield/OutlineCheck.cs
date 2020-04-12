using UnityEngine;

//Display element for shield around bot bricks
public class OutlineCheck : MonoBehaviour
{
    //Components
    SpriteRenderer spriteRenderer;

    //Init
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //Update shield outline colour to match shield level
    public void SetShieldColor(Color shieldColor)
    {
        spriteRenderer.color = shieldColor;
    }

    //Destroy shield outline and remove it from checks performed by shield bricks
    public void RemoveShieldOutline()
    {
        Destroy(gameObject);
        this.enabled = false;
    }
}
