namespace StarSalvager.Audio.Interfaces
{
    public enum AUDIO_STATE
    {
        NONE,
        IDLE,
        ANTICIPATION
    }
    
    public interface IUseAudioStates
    {
        AUDIO_STATE CurrentAudioState { get; }
        AUDIO_STATE PreviousAudioState { get; }
        void SetAudioState(in AUDIO_STATE newAudioState);
        void UpdateAudioState();
        void CleanAudioState();
    }
}
