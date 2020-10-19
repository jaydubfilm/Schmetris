namespace StarSalvager
{
    public interface IPart: ILevel
    {
        bool Destroyed { get;}
        bool Disabled { get; }
        PART_TYPE Type { get; set; }

        
    }
}

