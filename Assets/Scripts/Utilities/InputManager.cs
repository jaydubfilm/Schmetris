using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Utilities.Inputs
{
    public class InputManager : Singleton<InputManager>, IInput
    {
        private Bot[] _bots;
        private ScrapyardBot[] _scrapyardBots;

        private ObstacleManager obstacleManager
        {
            get
            {
                if (_obstacleManager == null)
                    _obstacleManager = FindObjectOfType<ObstacleManager>();

                return _obstacleManager;
            }
        }
        private ObstacleManager _obstacleManager;

        private EnemyManager enemyManager
        {
            get
            {
                if (_enemyManager == null)
                    _enemyManager = FindObjectOfType<EnemyManager>();
                return _enemyManager;
            }
        }
        private EnemyManager _enemyManager;

        private CameraController cameraController
        {
            get
            {
                if (_cameraController == null)
                    _cameraController = FindObjectOfType<CameraController>();

                return _cameraController;
            }
        }
        private CameraController _cameraController;

        //============================================================================================================//

        public void InitInput()
        {
            if (_bots == null || _bots.Length == 0)
                _bots = FindObjectsOfType<Bot>();

            if (_scrapyardBots == null || _scrapyardBots.Length == 0)
                _scrapyardBots = FindObjectsOfType<ScrapyardBot>();

            DeInitInput();

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
        
        //============================================================================================================//

        float _prevMove = 0.0f;

        private void SideMovement(InputAction.CallbackContext ctx)
        {
            var move = ctx.ReadValue<float>();
            _prevMove = move;

            var noObstacles = obstacleManager is null;
            
            foreach (var bot in _bots)
            {
                bot.Move(move, noObstacles);
            }

            if (noObstacles)
                return;

            obstacleManager.Move(move);
            enemyManager.Move(move);
            cameraController.Move(move);
            LevelManager.Instance.ProjectileManager.Move(move);

            StartCoroutine(dasTimer(move));
        }

        private void SideMovement(float move)
        {
            var noObstacles = obstacleManager is null;

            foreach (var bot in _bots)
            {
                bot.Move(move, noObstacles);
            }


            if (noObstacles)
                return;

            obstacleManager.Move(move);
            enemyManager.Move(move);
            cameraController.Move(move);

            LevelManager.Instance.ProjectileManager.Move(move);
        }

        IEnumerator dasTimer(float direction)
        {
            SideMovement(0.0f);
            yield return new WaitForSeconds(0.15f);

            if (_prevMove == direction)
            {
                SideMovement(direction);
            }
        }

        private void Rotate(InputAction.CallbackContext ctx)
        {
            var rot = ctx.ReadValue<float>();

            foreach (var bot in _bots)
            {
                bot.Rotate(rot);
            }

            foreach (var scrapyardBot in _scrapyardBots)
            {
                scrapyardBot.Rotate(rot);
            }
        }
        
        //============================================================================================================//
    }
}
