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
    public class SleeperMineEnemy : Enemy, IOverrideRecycleType
    {
        // Start is called before the first frame update
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;

        //public float Damage;
        //public float Radius;
        
        public override void LateInit()
        {
            base.LateInit();
            
            SetState(STATE.IDLE);
        }


        //State Functions
        //====================================================================================================================//

        #region State Functions

        private float AnticipationWaitTime = 1f;

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
                    LevelManager.Instance.BotObject.TryAOEDamageFrom(worldPosition, radius, damage);
                    SetState(STATE.DEATH);
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
            Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
            transform.position -= fallAmount;
            
            switch (currrentState)
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
            AnticipationWaitTime = 1f;
        }

        //====================================================================================================================//
        
        private void AnticipationState()
        {
            if (AnticipationWaitTime > 0f)
            {
                AnticipationWaitTime -= Time.deltaTime;
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
            if (!gameObject.CompareTag("Player"))
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
            
            LevelManager.Instance.DropLoot(m_enemyData.rdsTable.rdsResult.ToList(), transform.localPosition, true);
            
            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            LevelManager.Instance.EnemyManager.RemoveEnemy(this);

            SetState(STATE.ATTACK);
        }

        #endregion //IHealth Override

        //====================================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            CleanStateData();
            
            base.CustomRecycle(args);
        }

        public Type GetOverrideType()
        {
            return typeof(SleeperMineEnemy);
        }
    }

}