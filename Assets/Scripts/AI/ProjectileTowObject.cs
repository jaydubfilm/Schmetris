﻿using System;
using Recycling;
using Sirenix.OdinInspector;

namespace StarSalvager.AI
{
    //TODO: Handle proper setting of the collision tag
    public class ProjectileTowObject : Projectile, IOverrideRecycleType
    {

       // public GameObject towObject;
       [ReadOnly]
        public Actor2DBase towObjectActor;
        
        protected override void Update()
        {
            base.Update();

            if (IsRecycled)
            {
                return;
            }

            if (towObjectActor is null || towObjectActor.IsRecycled)
            {
                //towObject = null;
                towObjectActor = null;
                Recycler.Recycle<ProjectileTowObject>(this);
                return;
            }

            //IAttachable attachable = towObject.GetComponent<IAttachable>();
            if (towObjectActor is IAttachable attachable && attachable.Attached)
            {
                //towObject = null;
                towObjectActor = null;
                Recycler.Recycle<ProjectileTowObject>(this);
                return;
            }

            towObjectActor.transform.position = transform.position;
        }
        
        //============================================================================================================//

        /*protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            return;
        }*/

        //============================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            //If the towed object has not been recycled, then we can recyle it
            if (towObjectActor != null && !towObjectActor.IsRecycled)
            {
                switch (towObjectActor)
                {
                    case JunkBit junkBit:
                        Recycler.Recycle<JunkBit>(junkBit);
                        break;
                    case Bit bit:
                        Recycler.Recycle<Bit>(bit);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(towObjectActor), towObjectActor, null);
                }
            }

            //towObject = null;
            towObjectActor = null;

            base.CustomRecycle(args);
        }

        public Type GetOverrideType()
        {
            return typeof(ProjectileTowObject);
        }
    }
}