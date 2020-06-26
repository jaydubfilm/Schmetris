using StarSalvager.Utilities.JsonDataTypes;

namespace StarSalvager
{
    public interface ISaveable
    {
        BlockData ToBlockData();
        void LoadBlockData(BlockData blockData);
    }
}

