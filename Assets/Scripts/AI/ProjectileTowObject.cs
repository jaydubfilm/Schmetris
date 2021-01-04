﻿using UnityEngine;
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
    public class ProjectileTowObject : Projectile, ICustomRecycle
    {

        public GameObject towObject;
        public Actor2DBase towObjectIRecycledReference;
        
        protected override void Update()
        {
            base.Update();

            if (this.IsRecycled)
            {
                return;
            }

            if (towObject == null || towObjectIRecycledReference.IsRecycled)
            {
                towObject = null;
                towObjectIRecycledReference = null;
                Recycler.Recycle<ProjectileTowObject>(this);
                return;
            }

            IAttachable attachable = towObject.GetComponent<IAttachable>();
            if (attachable != null && attachable.Attached)
            {
                towObject = null;
                towObjectIRecycledReference = null;
                Recycler.Recycle<ProjectileTowObject>(this);
                return;
            }

            towObject.transform.position = transform.position;
        }
        
        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            return;
        }

        //============================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            if (towObject != null && !towObjectIRecycledReference.IsRecycled)
            {
                GameObject.Destroy(towObject);
            }

            towObject = null;
            towObjectIRecycledReference = null;

            base.CustomRecycle(args);
        }
    }
}