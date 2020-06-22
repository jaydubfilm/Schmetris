using UnityEngine.InputSystem;

namespace StarSalvager.Utilities.Inputs
{
    public class InputManager : SceneSingleton<InputManager>, IInput
    {
        private Bot[] _bots;

        private void Start()
        {
            if (_bots == null || _bots.Length == 0)
                _bots = FindObjectsOfType<Bot>();
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

            foreach (var bot in _bots)
            {
                bot.Move(move);
            }
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

