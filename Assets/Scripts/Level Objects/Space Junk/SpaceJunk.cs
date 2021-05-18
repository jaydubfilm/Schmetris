using System;
using System.Collections.Generic;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using UnityEngine;
using Random = UnityEngine.Random;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Values;
using System.Linq;
using StarSalvager.Prototype;

namespace StarSalvager
{
    public class SpaceJunk : CollidableBase, IHealth, IObstacle, ICanBeHit, IRotate
    {

        //IRotate properties
        //============================================================================================================//

        public bool Rotating { get; private set; }

        public int RotateDirection { get; private set; } = 1;


        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; private set; }

        //IObstacle Properties
        //============================================================================================================//

        public bool CanMove => true;

        public bool IsRegistered { get; set; }

        public bool IsMarkedOnGrid { get; set; } = false;

        //Asteroid Properties
        //====================================================================================================================//
        
        public List<int> RDSTableOdds { get; set; }

        public List<RDSTable> RDSTables { get; set; }

        public float Radius { get; private set; }

        public SpriteMask SpriteMask
        {
            get
            {
                if (_spriteMask == null)
                    _spriteMask = GetComponent<SpriteMask>();

                return _spriteMask;
            }
        }
        private SpriteMask _spriteMask;

        public void Start()
        {
            SetupHealthValues(10, 10);
        }

        //IRotate Functions
        //============================================================================================================//

        public void SetRotating(bool isRotating)
        {
            Rotating = isRotating;
            
            //Only need to set the rotation value when setting rotation to true
            if(Rotating)
                RotateDirection = Random.Range(-1, 2);
        }

        //IHealth Functions
        //============================================================================================================//

        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
            
            SetColor(Color.white);
        }

        public void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (CurrentHealth > 0) 
                return;

            //Spawns loot
            for (int i = 0; i < RDSTables.Count; i++)
            {
                int randomRoll = Random.Range(1, 101);
                if (randomRoll > RDSTableOdds[i])
                {
                    continue;
                }

                LevelManager.Instance.DropLoot(RDSTables[i].rdsResult.ToList(), transform.localPosition, true);
            }

            Recycler.Recycle<SpaceJunk>(this);
        }

        //ICanBeHit Functions
        //============================================================================================================//
        
        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);

            CreateExplosionEffect(worldPosition);

            return true;
        }

        //CollidableBase Functions
        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            //Debug.Break();
            
            var bot = gameObject.GetComponent<Bot>();

            if (bot != null)
            {
                CreateImpactEffect(worldHitPoint);
                //FIXME Should not be using this here
                AudioController.PlaySound(SOUND.ASTEROID_BASH);
                bot.TryHitAt(worldHitPoint, 5);
                Recycler.Recycle<SpaceJunk>(this);
                return;
            }

            var projectile = gameObject.GetComponent<Projectile>();

            if (projectile != null)
            {

            }
        }

        //Actor2DBase Functions
        //====================================================================================================================//
        
        public override void SetSprite(Sprite sprite)
        {
            base.SetSprite(sprite);

            SpriteMask.sprite = sprite;

            UpdatePhysicsShape(sprite);
        }

        //Asteroid Functions
        //============================================================================================================//

        public void SetRadius(float radius)
        {
            Radius = radius;
        }

        private void UpdatePhysicsShape(in Sprite sprite)
        {
            if (!(collider is PolygonCollider2D polygonCollider))
                throw new Exception();

            
            
            polygonCollider.pathCount = sprite.GetPhysicsShapeCount();

            var path = new List<Vector2>();
            for (var i = 0; i < polygonCollider.pathCount; i++)
            {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                polygonCollider.SetPath(i, path.ToArray());
            }
        }

        private void CreateImpactEffect(Vector2 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var eulerRotation = Vector3.forward * Random.Range(0, 360);

            var effect = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.IMPACT);
            var effectTransform = effect.transform;
            
            effectTransform.SetParent(transform);
            effectTransform.localPosition = localPosition;
            effectTransform.eulerAngles = eulerRotation;

            var time = effect.GetComponent<ScaleColorSpriteAnimation>().AnimationTime;
            
            Destroy(effect, time);
        }
        
        
        //ICustomRecycle Function
        //====================================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);
            
            transform.rotation = Quaternion.identity;
            SetRotating(false);

            renderer.sortingOrder = 0;
            Radius = 0f;
        }

        #region UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (transform == null)
                return;
            
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        #endregion //UNITY_EDITOR
    }
}
