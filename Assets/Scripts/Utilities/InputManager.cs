﻿using UnityEngine.InputSystem;

namespace StarSalvager.Utilities.Inputs
{
    public class InputManager : SceneSingleton<InputManager>, IInput
    {
        private Bot[] _bots;
        private ObstacleManager _obstacleManager;
        private EnemyManager _enemyManager;
        private CameraController _cameraController;

        private void Start()
        {
            if (_bots == null || _bots.Length == 0)
                _bots = FindObjectsOfType<Bot>();

            if (_obstacleManager == null)
                _obstacleManager = FindObjectOfType<ObstacleManager>();

            if (_enemyManager == null)
                _enemyManager = FindObjectOfType<EnemyManager>();

            if (_cameraController == null)
                _cameraController = FindObjectOfType<CameraController>();

            InitInput();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DeInitInput();
        }

        public void InitInput()
        {

            Input.Actions.Default.SideMovement.Enable();
            Input.Actions.Default.SideMovement.performed += SideMovement;

            Input.Actions.Default.Rotate.Enable();
            Input.Actions.Default.Rotate.performed += Rotate;

        }

        public void DeInitInput()
        {
            Input.Actions.Default.SideMovement.Disable();
            Input.Actions.Default.SideMovement.performed -= SideMovement;

            Input.Actions.Default.Rotate.Disable();
            Input.Actions.Default.Rotate.performed -= Rotate;
        }

        private void SideMovement(InputAction.CallbackContext ctx)
        {
            var move = ctx.ReadValue<float>();

            var noObstacles = _obstacleManager is null;
            
            foreach (var bot in _bots)
            {
                bot.Move(move, noObstacles);
            }


            if (noObstacles)
                return;

            _obstacleManager.Move(move);
            _enemyManager.Move(move);
            _cameraController.Move(move);
            LevelManager.Instance.ProjectileManager.Move(move);
        }

        private void Rotate(InputAction.CallbackContext ctx)
        {
            var rot = ctx.ReadValue<float>();

            foreach (var bot in _bots)
            {
                bot.Rotate(rot);
            }
        }
    }
}