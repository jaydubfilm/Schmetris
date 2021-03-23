using System.Collections;
using System.Collections.Generic;
using Recycling;
using StarSalvager;
using StarSalvager.Factories;
using UnityEngine;

public class BombProjectile : CollidableBase
{

    //[SerializeField]
    private float _damage;
    
    //====================================================================================================================//
    
    private void Update()
    {
        //TODO Check if below the screen
        if (IsRecycled)
            return;

        if (Position.y > 0f)
            return;
        
        CreateExplosionEffect(Position);
        Recycler.Recycle<BombProjectile>(this);
    }
    
    protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
    {
        if (!(gameObject.GetComponent<Bot>() is Bot bot))
            return;

        bot.TryHitAt(worldHitPoint, _damage);

        CreateExplosionEffect(worldHitPoint);
        Recycler.Recycle<BombProjectile>(this);
    }


    //====================================================================================================================//

    public void Init(in float damage)
    {
        _damage = damage;
    }

    //====================================================================================================================//

}
