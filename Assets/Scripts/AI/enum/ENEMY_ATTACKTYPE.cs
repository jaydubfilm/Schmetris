using System;

namespace StarSalvager.AI
{
    [Serializable]
    public enum ENEMY_ATTACKTYPE : int
    {
        Forward,
        AtPlayer,
        AtPlayerCone,
        Down,
        Random_Spray,
        Spiral,
        None,
        Fixed_Spray
    }
}