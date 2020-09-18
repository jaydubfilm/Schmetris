using System;

namespace StarSalvager
{
    [Serializable]
    public enum FACILITY_TYPE : int
    {
        FREEZER,
        STORAGEFUEL,
        STORAGEELECTRICITY,
        STORAGEPLASMA,
        STORAGESCRAP,
        STORAGEWATER,
        WORKBENCHCHIP,
        WORKBENCHCOIL,
        WORKBENCHFUSOR,
        REFINERY
    }
}