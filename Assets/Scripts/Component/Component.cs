using System;
using Recycling;
using StarSalvager.Factories;
using StarSalvager.Utilities.Particles;
using StarSalvager.Utilities.Saving;
using UnityEngine;

namespace StarSalvager
{
    public class Component : CollidableBase, IObstacle, IAdditiveMove, ICustomRecycle
    {

        public int GearNum { get; set; }

        //IObstacle Properties
        //============================================================================================================//

        public bool CanMove => true;

        public bool IsRegistered { get; set; }

        public bool IsMarkedOnGrid { get; set; }

        public Vector2 AddMove => GetTowardsPlayer();
        private float _speed;
        

        //====================================================================================================================//
        
        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            Color color = Color.HSVToRGB(0.11944f, 0.94f, 0.98f);

            var bot = gameObject.GetComponent<Bot>();

            if (bot == null)
            {
                return;
            }

            PlayerDataManager.AddGears(FactoryManager.Instance.GetFactory<ComponentFactory>().GetNumComponentsGained());
            FloatingText.Create($"+{FactoryManager.Instance.GetFactory<ComponentFactory>().GetNumComponentsGained()}", transform.position, color);

            Recycler.Recycle<Component>(this);

        }

        //====================================================================================================================//

        private Vector3 GetTowardsPlayer()
        {
            if (IsRecycled || GameManager.IsState(GameState.LevelBotDead))
                return Vector3.zero;
            
            var playerLocation = LevelManager.Instance.BotInLevel.transform.position;
            var direction = (Vector2)(playerLocation - transform.position).normalized;

            _speed += 0.2f;

            return direction * _speed;
        }

        //====================================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            _speed = 0f;
        }
    }
}
