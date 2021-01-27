using System;
using Recycling;
using StarSalvager.Factories;
using StarSalvager.Utilities.Particles;
using StarSalvager.Utilities.Saving;
using UnityEngine;

namespace StarSalvager
{
    public class Component : CollidableBase, IObstacle
    {
        
        public int GearNum { get; set; }

        //IObstacle Properties
        //============================================================================================================//

        public bool CanMove => true;

        public bool IsRegistered { get; set; }

        public bool IsMarkedOnGrid { get; set; }

        
        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();

            if (bot == null)
            {
                return;
            }

            PlayerDataManager.AddComponent(GearNum);
            FloatingText.Create($"+{GearNum}", transform.position, Color.green);

            Recycler.Recycle<Component>(this);

        }
    }
}

