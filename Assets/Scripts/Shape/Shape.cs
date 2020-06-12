using System.Collections.Generic;
using StarSalvager;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

[RequireComponent(typeof(CompositeCollider2D), typeof(Rigidbody2D))]
public class Shape : CollidableBase
{
    protected new Rigidbody2D rigidbody
    {
        get
        {
            if (_rigidbody != null) 
                return _rigidbody;
            
            _rigidbody = gameObject.GetComponent<Rigidbody2D>();
            return _rigidbody;
        }
    }
    private Rigidbody2D _rigidbody;
    
    //================================================================================================================//

    public BIT_TYPE BitType { get; private set; }

    private List<Bit> attachedBits => _attachedBits ?? (_attachedBits = new List<Bit>());
    private List<Bit> _attachedBits;

    public void Setup(BIT_TYPE bitType, List<Bit> bits)
    {
        SetBitType(bitType);

        foreach (var bit in bits)
        {
            attachedBits.Add(bit);
        }
    }

    public void SetBitType(BIT_TYPE bitType)
    {
        BitType = bitType;
    }

    //FIXME Need to setup the global variables for BitSize
    public void PushNewBit(Bit bit, DIRECTION direction)
    {
        
        var newCoord = direction.ToVector2Int();

        if (attachedBits.Count == 0)
        {
            newCoord = Vector2Int.zero;
        }
        else
        {
            attachedBits.CoordinateOccupied(direction, ref newCoord);

        }
        

        bit.Coordinate = newCoord;
        bit.SetAttached(true);
        bit.transform.position = transform.position + (Vector3)(Vector2.one * newCoord * 1.28f);
        bit.transform.SetParent(transform);
            
        attachedBits.Add(bit);
    }
    public void PushNewBit(Bit bit, DIRECTION direction, int fromIndex)
    {
        
    }
    
    //================================================================================================================//

    protected override void OnCollide(Bot bot)
    {
        
    }
    
    //================================================================================================================//

}
