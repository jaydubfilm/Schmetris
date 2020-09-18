using System;

namespace StarSalvager
{
    [Serializable]
    public enum FACILITY_TYPE : int
    {
        WORKBENCH,
        FREEZER,
        STORAGEFUEL,
        STORAGEWATER,
        STORAGEPLASMA,
        STORAGESCRAP,
        STORAGEELECTRICITY,
        REFINERY
    }
}