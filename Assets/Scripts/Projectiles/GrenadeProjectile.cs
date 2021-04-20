using Recycling;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using UnityEngine;

namespace StarSalvager.Projectiles
{
    public class GrenadeProjectile : Actor2DBase
    {
        private Vector3 _startPos, _endPos;
        private float _speed;
        private float _damage;
        private float _range;

        //====================================================================================================================//
        
        public void Init(in Vector3 startPos, in Vector3 endPos, in float speed, in float damage, in float range)
        {
            _startPos = startPos;
            _endPos = endPos;
            _speed = speed;
            _damage = damage;
            _range = range;

            transform.position = _startPos;
        }

        private void Update()
        {
            var distance = Vector2.Distance(Position, _endPos);

            if (distance <= 0.1f)
            {
                CreateBombEffect(Position, _range);
                AudioController.PlaySound(SOUND.BOMB_BLAST);
                
                var enemies = LevelManager.Instance.EnemyManager.GetEnemiesInRange(Position, _range);

                foreach (var enemy in enemies)
                {
                    enemy.TryHitAt(enemy.Position, _damage);
                } 
                
                Recycler.Recycle<GrenadeProjectile>(this);
                return;
            }

            transform.position = Vector2.MoveTowards(Position, _endPos, _speed * Time.deltaTime);
        }

        //====================================================================================================================//
        
        private static void CreateBombEffect(in Vector3 position, in float range)
        {
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.BOMB);

            effect.transform.position = position;
            
            var effectAnimationComponent = effect.GetComponent<ParticleSystemGroupScaling>();
            
            effectAnimationComponent.SetSimulationSize(range);
            
            Destroy(effect, effectAnimationComponent.AnimationTime);
        }
    }
}
