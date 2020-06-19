using System;

namespace StarSalvager.AI
{
    [Serializable]
    public enum ENEMY_MOVETYPE : int
    {
        Standard,
        Oscillate,
        OscillateHorizontal,
        Orbit,
        Horizontal,
        HorizontalDescend,
        Down
    }
}