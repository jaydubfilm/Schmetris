namespace StarSalvager.Utilities.Analytics.SessionTracking.Data
{
    public readonly struct PartSelectionData
    {
        public static readonly PartSelectionData Empty = new PartSelectionData(PART_TYPE.EMPTY, default);
        public readonly PART_TYPE Selected;
        public readonly PART_TYPE[] Options;

        public PartSelectionData(PART_TYPE selected, PART_TYPE[] options)
        {
            Selected = selected;
            Options = options;
        }
    }
}
