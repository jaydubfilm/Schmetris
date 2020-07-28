namespace StarSalvager.Utilities
{
    public interface IPausable
    {
        bool isPaused { get; }

        void RegisterPausable();
        void OnResume();
        void OnPause();
    }
}

