using System;

namespace StarSalvager
{
    [Serializable]
    public enum ENEMY_ATTACKTYPE : int
    {
        Forward,
        AtPlayer,
        AtPlayerCone,
        Down,
        Spray,
        Spiral,
        None
    }
}