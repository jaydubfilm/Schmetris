using Recycling;
using StarSalvager.Audio;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.Particles;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager
{
    public class GearCollectable : CollidableBase, IObstacle, IAdditiveMove, IHasBounds, ISimpleAnimation
    {

        //ISimpleAnimation Properties
        //====================================================================================================================//

        public SimpleAnimator SimpleAnimator
        {
            get
            {
                if (_simpleAnimator is null)
                    _simpleAnimator = GetComponent<SimpleAnimator>();

                return _simpleAnimator;
            }
        }

        private SimpleAnimator _simpleAnimator;


        //====================================================================================================================//

        public int GearNum { get; set; }

        //IObstacle Properties
        //============================================================================================================//

        public bool CanMove => true;

        public bool IsRegistered { get; set; }

        public bool IsMarkedOnGrid { get; set; }

        public Vector2 AddMove => GetDeltaTowardsPlayer();
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

            PlayerDataManager.AddGears(GearNum);
            FloatingText.Create($"+{GearNum}", transform.position, color);
            AudioController.PlaySound(SOUND.COLLECT_GEARS);

            Recycler.Recycle<GearCollectable>(this);

        }

        //====================================================================================================================//

        private Vector3 GetDeltaTowardsPlayer()
        {
            if (IsRecycled || GameManager.IsState(GameState.LevelBotDead) || HintManager.ShowingHint || GameTimer.IsPaused)
                return Vector3.zero;

            var playerLocation = LevelManager.Instance.BotInLevel.transform.position;
            var direction = (Vector2) (playerLocation - Position).normalized;

            _speed += 0.2f;

            return direction * _speed;
        }

        //====================================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);

            _speed = 0f;
        }

        //IHasBounds Functions
        //====================================================================================================================//

        public Bounds GetBounds()
        {
            return new Bounds
            {
                center = transform.position,
                size = Vector2.one * Constants.gridCellSize
            };
        }

        //====================================================================================================================//
    }

}
