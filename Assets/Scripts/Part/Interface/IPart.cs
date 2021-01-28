namespace StarSalvager
{
    public interface IPart //: ILevel
    {
        //bool Destroyed { get;}
        bool Disabled { get; }
        PART_TYPE Type { get; set; }

        PatchData[] Patches { get; set; }

        void AddPatch(in PatchData patchData);
        void RemovePatch(in PatchData patchData);
    }

}

