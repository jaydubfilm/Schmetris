using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Structs;

namespace StarSalvager.Utilities.JSON.Converters
{
    public class ComboRecordDataConverter : JsonConverter
    {
        
        private struct JsonComboRecord
        {
            public int bit;
            public int combo;
            public int level;
            public int count;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = (Dictionary<ComboRecordData, int>) value;
            var outList = new List<JsonComboRecord>();
            foreach (var i in val)
            {
                
                var data = new JsonComboRecord
                {
                    bit = (int)i.Key.BitType,
                    combo = (int)i.Key.ComboType,
                    level = i.Key.FromLevel,
                    count = i.Value
                };

                outList.Add(data);
            }
            

            serializer.Serialize(writer, outList);
        }

        public override bool CanConvert(Type objectType)
        {
            var isType = objectType == typeof(Dictionary<ComboRecordData, int>) ||
                         objectType == typeof(IReadOnlyDictionary<ComboRecordData, int>);
            return isType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = serializer.Deserialize(reader, typeof(List<JsonComboRecord>));

            if (data == null) return default;

            var comboRecord = (List<JsonComboRecord>) data;

            var outDict = new Dictionary<ComboRecordData, int>();
            foreach (var jsonComboRecord in comboRecord)
            {
                outDict.Add(new ComboRecordData
                {
                    BitType = (BIT_TYPE) jsonComboRecord.bit,
                    ComboType = (COMBO) jsonComboRecord.combo,
                    FromLevel = jsonComboRecord.level,

                }, jsonComboRecord.count);
            }

            return outDict;
        }
    }
}
