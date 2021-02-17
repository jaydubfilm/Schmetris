using StarSalvager.Values;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Particles;
using UnityEngine;


namespace StarSalvager
{
    public class DecoyDrone : CollidableBase, IHealth, ICanBeHit
    {
        //[NonSerialized]
        //public Bot bot;

        //IHealth Properties
        //====================================================================================================================//

        public float StartingHealth { get; private set; }
        public float CurrentHealth { get; private set; }

        //DecoyDrone Properties
        //====================================================================================================================//

        private Bot _bot;

        //private float _timer = 0.0f;
        //private float m_timeAlive = 5.0f;
        private Vector2 m_positionMoveUpwards;

        //Unity Functions
        //====================================================================================================================//

        // Update is called once per frame
        private void Update()
        {
            /*if (_timer >= m_timeAlive)
            {
                bot.DecoyDrone = null;
                Destroy(gameObject);
            }*/

            if (transform == null)
                return;
            
            transform.position = Vector2.Lerp(transform.position, m_positionMoveUpwards, Time.deltaTime);
            /*_timer += Time.deltaTime;*/
        }
        
        //DecoyDrone Functions
        //====================================================================================================================//

        public void Init(in Bot bot, in float speed)
        {
            _bot = bot;
            m_positionMoveUpwards = (Vector2) transform.position + (Vector2.up * (speed * Constants.gridCellSize));
        }

        //IHealth Functions
        //====================================================================================================================//
        
        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
        }

        public void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            FloatingText.Create($"{amount}", transform.position, amount > 0 ? Color.green : Color.red);
            
            if (CurrentHealth > 0)
                return;
            
            _bot.DecoyDrone = null;
            Destroy(gameObject);
        }

        //CollidableBase Functions
        //====================================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint) { }

        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);
            
            var explosion = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.EXPLOSION);
            explosion.transform.position = worldPosition;
            
            var particleScaling = explosion.GetComponent<ParticleSystemGroupScaling>();
            var time = particleScaling.AnimationTime;

            Destroy(explosion, time);
            
            if(CurrentHealth > 0)
                AudioController.PlaySound(SOUND.ENEMY_IMPACT);

            return true;
        }
    }
}