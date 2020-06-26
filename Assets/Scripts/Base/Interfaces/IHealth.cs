using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IHealth
    {
        float StartingHealth { get; }
        float CurrentHealth { get; }
        void SetupHealthValues(float startingHealth, float currentHealth);
        void ChangeHealth(float amount);
    }
}


