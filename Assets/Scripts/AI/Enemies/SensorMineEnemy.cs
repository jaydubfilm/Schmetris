using System;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class SensorMineEnemy  : Enemy, IPlayEnemySounds<SensorMineSounds>
    {
        private enum AUDIO_STATE
        {
            NONE,
            IDLE,
            WARNING
        }
        
        public SensorMineSounds EnemySound => (SensorMineSounds) EnemySoundBase;
        public float anticipationTime = 1f;
        public float triggerDistance = 5f;

        private float _distanceToPlayer;
        
        public float minSoundThreshold;
        public float idleSoundDistance;
        public float warningSoundDistance;
        
        //====================================================================================================================//
        
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        //====================================================================================================================//

        private float _anticipationTime;
        private Vector2 _playerPosition;
        private AudioSource _audioSource;
        private AUDIO_STATE _audioState = AUDIO_STATE.NONE;
        private AUDIO_STATE _previousAudioState = AUDIO_STATE.NONE;
        

        //====================================================================================================================//
        
        public override void LateInit()
        {
            EnemySoundBase = AudioController.Instance.SensorMineSounds;
            
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
                    break;
                case STATE.ANTICIPATION:
                    //TODO Change animation to Anticipation Animation
                    _anticipationTime = anticipationTime;
                    break;
                case STATE.ATTACK:
                    var worldPosition = transform.position;
                    var damage = FactoryManager.Instance.MineRemoteData.MineMaxDamage;
                    var radius = FactoryManager.Instance.MineRemoteData.MineMaxDistance;
                    //TODO Spawn explosion effect

                    CreateFreezeEffect(worldPosition, radius * 2);
                    //Do damage to relevant blocks
                    LevelManager.Instance.BotInLevel.TryAOEDamageFrom(worldPosition, radius, damage, true);
                    DestroyEnemy();
                    EnemySound.attackSound.Play();
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
            ApplyFallMotion();

            _distanceToPlayer = Vector2.Distance(transform.position, _playerPosition);
            UpdateAudioState();

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

            CleanAudio();
        }

        //====================================================================================================================//

        private void SearchState()
        {
            if (_distanceToPlayer > triggerDistance)
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

        private void UpdateAudioState()
        {
            //--------------------------------------------------------------------------------------------------------//
            
            void SetAudioState(in AUDIO_STATE newAudioState)
            {
                if (newAudioState == _audioState) return;
                
                _previousAudioState = _audioState;
                _audioState = newAudioState;
                
                if (_previousAudioState != newAudioState)
                {
                    switch (_previousAudioState)
                    {
                        case AUDIO_STATE.NONE: break;
                        case AUDIO_STATE.IDLE:
                            EnemySound.idleLoop.Stop();
                            _audioSource = null;
                            break;
                        case AUDIO_STATE.WARNING:
                            EnemySound.warningLoop.Stop();
                            _audioSource = null;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                switch (newAudioState)
                {
                    case AUDIO_STATE.NONE: break;
                    case AUDIO_STATE.IDLE:
                        EnemySound.idleLoop.Play(out _audioSource);
                        break;
                    case AUDIO_STATE.WARNING:
                        EnemySound.warningLoop.Play(out _audioSource);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(newAudioState), newAudioState, null);
                }
            }

            //--------------------------------------------------------------------------------------------------------//
            
            if (_distanceToPlayer > minSoundThreshold)
            {
                SetAudioState(AUDIO_STATE.NONE);
            }
            else if (_distanceToPlayer > idleSoundDistance && _distanceToPlayer > warningSoundDistance )
            {
                SetAudioState(AUDIO_STATE.IDLE);
                _audioSource.volume =
                    Mathf.InverseLerp(minSoundThreshold, idleSoundDistance, _distanceToPlayer) * EnemySound.idleLoop.volume;

            }
            else if (_distanceToPlayer < idleSoundDistance && _distanceToPlayer > warningSoundDistance)
            {
                SetAudioState(AUDIO_STATE.WARNING);
                _audioSource.volume = EnemySound.warningLoop.volume;
            }
        }

        private void CleanAudio()
        {
            _audioSource = null;
            switch (_audioState)
            {
                case AUDIO_STATE.NONE:
                    break;
                case AUDIO_STATE.IDLE:
                    EnemySound.idleLoop.Stop();
                    break;
                case AUDIO_STATE.WARNING:
                    EnemySound.warningLoop.Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _previousAudioState = _audioState = AUDIO_STATE.NONE;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var position = Position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(position, minSoundThreshold);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, idleSoundDistance);
            Gizmos.color = new Color(0.99f, 0.5f, 0.1f);
            Gizmos.DrawWireSphere(position, warningSoundDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, triggerDistance);
        }
#endif

        //====================================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            _playerPosition = playerLocation;
            StateUpdate();
        }
        
        protected override void ApplyFleeMotion()
        {
            ApplyFallMotion();
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

            KilledEnemy(STATE.ATTACK);
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
