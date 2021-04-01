using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Utilities.Inputs
{
    public static class Input
    {
        public static SalvagerInput Actions => _actions ?? (_actions = new SalvagerInput());

        private static SalvagerInput _actions;
        

    }
}


