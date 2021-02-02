using System;

namespace StarSalvager.AI
{
    [Serializable, Obsolete("Use FIRE_TYPE instead")]
    public enum ENEMY_ATTACKTYPE : int
    {
        None = 6 ,

        Forward = 0,
        AtPlayer = 1,
        AtPlayerCone = 2,
        Down = 3,
        Random_Spray =4,
        Spiral = 5,
        Fixed_Spray = 7,
        Heat_Seeking
    }
}