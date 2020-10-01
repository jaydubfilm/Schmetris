namespace StarSalvager
{
    public interface ICanDetach
    {
        int AttachPriority { get; }
        bool PendingDetach { get; set; }
    }
}
