using StarSalvager.Audio.Enemies;

namespace StarSalvager.Audio.Interfaces
{
    public interface IPlayEnemySounds
    {
        EnemySoundBase EnemySoundBase { get; }
    }

    public interface IPlayEnemySounds<out T> : IPlayEnemySounds where T : EnemySoundBase
    {
        T EnemySound { get; }
    }
}
