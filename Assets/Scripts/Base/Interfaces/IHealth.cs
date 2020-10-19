namespace StarSalvager
{
    public interface IHealth
    {
        float StartingHealth { get; }
        float CurrentHealth { get; }
        void SetupHealthValues(float startingHealth, float currentHealth);
        void ChangeHealth(float amount);
    }

    public interface IHealthBoostable : IHealth
    {
        float BoostedHealth { get; }
        float BoostAmount { get; }
        void SetHealthBoost(float boostAmount);
    }
}


