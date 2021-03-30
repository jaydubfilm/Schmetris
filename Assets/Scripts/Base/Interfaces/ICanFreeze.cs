namespace StarSalvager
{
    internal interface ICanFreeze
    {
        bool Frozen { get; }
        float FreezeTime { get; }

        void SetFrozen(in float time);
    }
}
