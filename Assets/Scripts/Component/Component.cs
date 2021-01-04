using System;
using Recycling;
using UnityEngine;

namespace StarSalvager
{
    public class Component : CollidableBase, IObstacle, ICustomRecycle
    {
        
        //IObstacle Properties
        //============================================================================================================//

        public bool CanMove => true;

        public bool IsRegistered { get; set; }

        public bool IsMarkedOnGrid { get; set; }

        
        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();
            
            //TODO Collect the component
            throw new NotImplementedException($"Collecting {nameof(Component)} not yet implemented");

        }
        
        public virtual void CustomRecycle(params object[] args)
        {

        }

    }
}

