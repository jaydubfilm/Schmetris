using System.Collections;
using System.Collections.Generic;
using StarSalvager;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Sabre : CollidableBase
{
    private float _damage;
    
    public float minSize { get; private set; }
    public float maxSize { get; private set; }
    
    protected override string CollisionTag => "Enemy";

    protected override bool useCollisionStay => false;

    private BoxCollider2D BoxCollider2D => collider as BoxCollider2D;

    //====================================================================================================================//

    public void Init(in float damage, in float minSize, in float maxSize)
    {
        _damage = damage;
        
        this.minSize = minSize;
        this.maxSize = maxSize;

        SetSize(this.minSize);
    }

    //====================================================================================================================//

    public void SetSize(in float size)
    {
        var vSize = new Vector2(1, size);
        renderer.size = vSize;
        BoxCollider2D.size = vSize;
    }

    public void SetTransform(in Vector2 worldPosition, in Vector2 upDirection)
    {
        transform.position = worldPosition;
        transform.up = upDirection;
    }

    public void SetActive(in bool state)
    {
        gameObject.SetActive(state);
    }

    //====================================================================================================================//
    
    protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
    {
        //Debug.Log($"Collided with {gameObject.name} at {worldHitPoint}", gameObject);

        if (!(gameObject.GetComponent<ICanBeHit>() is ICanBeHit iCanBeHit))
            return;

        iCanBeHit.TryHitAt(worldHitPoint, _damage/* * Time.deltaTime*/);
    }
}
