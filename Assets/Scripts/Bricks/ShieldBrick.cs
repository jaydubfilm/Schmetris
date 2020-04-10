using UnityEngine;
using Sirenix.OdinInspector;

//Brick that reduces damage dealt to nearby bricks
public class ShieldBrick : MonoBehaviour
{
    //Components
    Brick parentBrick;
    BoxCollider2D box2D;
    ShieldTrigger childTrigger;
    SpriteRenderer outlineSR;

    //Shield info
    public int[] radiusAtLevel;
    public int[] healthAtLevel;
    public float[] percentBlockedAtLevel;
    public float[] rechargeSpeedAtLevel;

    //Handle changes to shield level while active
    bool hasShield = false;
    public float shieldHealthDiff = 0;
    float _shieldHp = 0;
    public float shieldHp
    {
        get
        {
            return _shieldHp;
        }
        set
        {
            _shieldHp = value;
            if (parentBrick)
            {
                _shieldHp = Mathf.Clamp(_shieldHp, 0, healthAtLevel[parentBrick.GetPoweredLevel()]);
                UpdateShieldColour();
                if(shieldHp == 0)
                {
                    ToggleShield(false);
                }
            }
        }
    }
    [Range(1, 5)]
    public int radius = 1;

    //Shield display
    public Color fullShieldColor = Color.green;
    public Color emptyShieldColor = Color.red;

    //Init
    void Start()
    {
        parentBrick = GetComponent<Brick>();
        childTrigger = GetComponentInChildren<ShieldTrigger>();
        box2D = childTrigger.GetComponent<BoxCollider2D>();
        shieldHp = healthAtLevel[parentBrick.GetPoweredLevel()];
    }

    //Shield radius converted to bot units
    float ConvertRadius(int radius)
    {
        float convertedRadius = (ScreenStuff.colSize * 3) + (ScreenStuff.colSize * 2 * (radius - 1));
        return convertedRadius;
    }

    //Recharge shield, or destroy it if resources have run out
    private void Update()
    {
        if(parentBrick.hasResources && GameController.Instance.bot.brickList.Contains(gameObject))
        {
            shieldHp += rechargeSpeedAtLevel[parentBrick.GetPoweredLevel()] * Time.deltaTime;
            ToggleShield(true);
        }
        else
        {
            ToggleShield(false);
        }

        if (radius != radiusAtLevel[GetComponent<Brick>().GetPoweredLevel()])
        {
            radius = radiusAtLevel[GetComponent<Brick>().GetPoweredLevel()];
            SetShieldSize();
        }
    }

    //Adjust shield colour based on current health
    public void UpdateShieldColour()
    {
        Color shieldColor = emptyShieldColor + (fullShieldColor - emptyShieldColor) * shieldHp / healthAtLevel[parentBrick.GetPoweredLevel()];
        foreach (Brick brick in childTrigger.protectedList)
        {
            if (brick != null && brick.GetComponentInChildren<OutlineCheck>())
            {
                brick.GetComponentInChildren<OutlineCheck>().GetComponent<SpriteRenderer>().color = shieldColor;
            }
        }
    }

    //Turn shield on or off
    public void ToggleShield(bool setActive)
    {
        if(hasShield != setActive)
        {
            hasShield = setActive;
            childTrigger.SetShieldOutline(hasShield);
        }
    }

    //Update size of shield trigger to match radius
    [Button]
    public void SetShieldSize()
    {
        if (box2D)
        {
            radius = radiusAtLevel[parentBrick.GetPoweredLevel()];
            float newSize = ConvertRadius(radius);
            box2D.size = new Vector2(newSize, newSize);
        }
    }
}
