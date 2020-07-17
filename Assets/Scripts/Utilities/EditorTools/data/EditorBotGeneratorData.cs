using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Factories.Data
{
    public class EditorBotGeneratorData : EditorGeneratorDataBase
    {
        public EditorBotGeneratorData(string name, List<BlockData> blockData) : base(name, blockData)
        {
            m_classType = nameof(EditorBotGeneratorData);
        }
    }
}