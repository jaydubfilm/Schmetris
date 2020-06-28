using System;

namespace StarSalvager.AI
{
    [Serializable]
    public enum ASTEROID_SIZE : int
    {
        Bit, // one bit
        Small, // 2-3 bits
        Medium, // 4-5 bits
        Large // 6-8 bits
    }
}