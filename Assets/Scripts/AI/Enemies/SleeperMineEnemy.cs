using System;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class SleeperMineEnemy : Enemy, IPlayEnemySounds<SleeperMineSounds>
    {
        public SleeperMineSounds EnemySound => (SleeperMineSounds) EnemySoundBase;
        
        public float anticipationTime = 1f;

        //====================================================================================================================//

        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;


        //====================================================================================================================//
        
        private float _anticipationTime;

        //====================================================================================================================//
        
        public override void LateInit()
        {
            base.LateInit();
            
            SetState(STATE.IDLE);
        }


        //State Functions
        //====================================================================================================================//

        #region State Functions


        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                case STATE.IDLE:
                    return;
                case STATE.ANTICIPATION:
                    //TODO Change animation to Anticipation Animation
                    break;
                case STATE.ATTACK:
                    var worldPosition = transform.position;
                    var damage = FactoryManager.Instance.MineRemoteData.MineMaxDamage;
                    var radius = FactoryManager.Instance.MineRemoteData.MineMaxDistance;
                    //TODO Spawn explosion effect
                    CreateBombEffect(worldPosition, radius * 2);
                    
                    //Do damage to relevant blocks
                    LevelManager.Instance.BotInLevel.TryAOEDamageFrom(worldPosition, radius, damage);
                    DestroyEnemy();
                    break;
                case STATE.DEATH:
                    //Recycle ya boy
                    Recycler.Recycle<SleeperMineEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateUpdate()
        {
            ApplyFallMotion();
            
            switch (currentState)
            {
                case STATE.NONE: 
                case STATE.IDLE:
                case STATE.ATTACK:
                case STATE.DEATH:
                    return;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void CleanStateData()
        {
            base.CleanStateData();
            _anticipationTime = anticipationTime;
        }

        //====================================================================================================================//
        
        private void AnticipationState()
        {
            if (_anticipationTime > 0f)
            {
                _anticipationTime -= Time.deltaTime;
                return;
            }
            
            SetState(STATE.ATTACK);
        }

        #endregion //State Functions

        //============================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
        }
        
        protected override void ApplyFleeMotion()
        {
            ApplyFallMotion();
        }

        #endregion

        //Effects
        //====================================================================================================================//
        
        private void CreateBombEffect(in Vector2 worldPosition, in float range)
        {
           
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.BOMB);

            effect.transform.position = worldPosition;
            
            var effectAnimationComponent = effect.GetComponent<ParticleSystemGroupScaling>();
            
            effectAnimationComponent.SetSimulationSize(range);
            
            Destroy(effect, effectAnimationComponent.AnimationTime);
        }

        //====================================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            if (!gameObject.CompareTag(TagsHelper.PLAYER))
                return;

            collider.enabled = false;
            SetState(STATE.ANTICIPATION);
        }

        //IHealth Override
        //====================================================================================================================//

        #region IHealth Override

        public override void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (amount < 0)
            {
                FloatingText.Create($"{Mathf.Abs(amount)}", transform.position, Color.red);
            }

            if (CurrentHealth > 0) 
                return;

            KilledEnemy(STATE.ATTACK);
        }
        
        #endregion //IHealth Override

        //====================================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            CleanStateData();
            
            base.CustomRecycle(args);
        }

        public override Type GetOverrideType()
        {
            return typeof(SleeperMineEnemy);
        }
    }

}