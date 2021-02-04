using System;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class SensorMineEnemy  : Enemy, IOverrideRecycleType
    {
        public float anticipationTime = 1f;
        public float triggerDistance = 5f;
        
        //====================================================================================================================//
        
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        //====================================================================================================================//

        private float _anticipationTime;
        private Vector2 _playerPosition;

        //====================================================================================================================//
        
        public override void LateInit()
        {
            base.LateInit();
            
            SetState(STATE.SEARCH);
        }

        //State Functions
        //====================================================================================================================//

        #region State Functions


        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                case STATE.SEARCH:
                case STATE.ANTICIPATION:
                    //TODO Change animation to Anticipation Animation
                    break;
                case STATE.ATTACK:
                    var worldPosition = transform.position;
                    var damage = FactoryManager.Instance.MineRemoteData.MineMaxDamage;
                    var radius = FactoryManager.Instance.MineRemoteData.MineMaxDistance;
                    //TODO Spawn explosion effect

                    CreateFreezeEffect(worldPosition, radius * 2);
                    //Do damage to relevant blocks
                    LevelManager.Instance.BotInLevel.TryAOEDamageFrom(worldPosition, radius, damage, true);
                    SetState(STATE.DEATH);
                    break;
                case STATE.DEATH:
                    //Recycle ya boy
                    Recycler.Recycle<SensorMineEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateUpdate()
        {
            Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
            transform.position -= fallAmount;
            m_mostRecentMovementDirection = Vector3.down;

            switch (currentState)
            {
                case STATE.NONE:
                case STATE.ATTACK:
                case STATE.DEATH:
                    return;
                case STATE.SEARCH:
                    SearchState();
                    break;
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

        private void SearchState()
        {
            var distance = Vector2.Distance(transform.position, _playerPosition);

            if (distance > triggerDistance)
                return;
            
            SetState(STATE.ANTICIPATION);
        }
        
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

        //====================================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            _playerPosition = playerLocation;
            StateUpdate();
        }
        

        #endregion

        //Effects
        //====================================================================================================================//
        
        private void CreateFreezeEffect(in Vector2 worldPosition, in float range)
        {
           
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.FREEZE);

            effect.transform.position = worldPosition;
            
            var effectAnimationComponent = effect.GetComponent<ParticleSystemGroupScaling>();
            
            effectAnimationComponent.SetSimulationSize(range);
            
            Destroy(effect, effectAnimationComponent.AnimationTime);
        }

        //IHealth Overrides
        //====================================================================================================================//
        
        public override void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (amount < 0)
            {
                FloatingText.Create($"{Mathf.Abs(amount)}", transform.position, Color.red);
            }

            if (CurrentHealth > 0) 
                return;

            DropLoot();
            
            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            LevelManager.Instance.EnemyManager.RemoveEnemy(this);
            
            SetState(STATE.ATTACK);
        }

        //====================================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            CleanStateData();
            
            base.CustomRecycle(args);
        }

        public override Type GetOverrideType()
        {
            return typeof(SensorMineEnemy);
        }

        //====================================================================================================================//
        
    }
}
