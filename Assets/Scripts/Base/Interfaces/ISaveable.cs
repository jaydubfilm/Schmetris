using StarSalvager.Utilities.JsonDataTypes;

namespace StarSalvager
{
    public interface ISaveable<TE>: ISaveable where TE: IBlockData
    {
        new TE ToBlockData();
    }
    public interface ISaveable
    {
        IBlockData ToBlockData();
        void LoadBlockData(IBlockData blockData);
    }
}

