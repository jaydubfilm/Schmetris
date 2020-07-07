namespace StarSalvager.Utilities
{
    public interface IPausable
    {
        bool isPaused { get; }
        void OnResume();
        void OnPause();
    }
}

