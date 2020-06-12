using System.Collections;
using System.Collections.Generic;
using StarSalvager;
using StarSalvager.Utilities.JsonDataTypes;
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

    public void SetBitType(BIT_TYPE bitType)
    {
        BitType = bitType;
    }

    public void PushNewBit(Bit bit, DIRECTION direction)
    {
        
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
