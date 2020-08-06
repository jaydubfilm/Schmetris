namespace StarSalvager
{
    public interface IRDSObject
    {
        double rdsProbability { get; set; } // The chance for this item to drop
        bool rdsUnique { get; set; }        // Only drops once per query
        bool rdsAlways { get; set; }        // Drops always
        bool rdsEnabled { get; set; }       // Can it drop now?
        RDSTable rdsTable { get; set; }     // What table am I in?
    }
}