using UnityEngine;
using StarSalvager.Factories.Data;
using System;
using Recycling;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Projectiles;
using StarSalvager.Utilities;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;

namespace StarSalvager.AI
{
    //TODO: Handle proper setting of the collision tag
    public class ProjectileTrigger : Projectile, ICustomRecycle
    {

        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();

            if (bot == null)
                return;

            if (bot.TryProjectileTriggerAt(transform.position, ProjectileData.ProjectileType))
                Recycler.Recycle<Projectile>(this);
        }

        //====================================================================================================================//
    }
}