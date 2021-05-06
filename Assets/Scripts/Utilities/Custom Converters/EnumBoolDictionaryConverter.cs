using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StarSalvager.Utilities.Puzzle.Structs;

namespace StarSalvager.Utilities.JSON.Converters
{
    public class EnumBoolDictionaryConverter<T> : JsonConverter where T: Enum
    {
        private struct JsonRecord
        {
            public int t;
            public int u;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = (Dictionary<T, bool>) value;
            var outList = new List<JsonRecord>();
            foreach (var i in val)
            {
                var data = new JsonRecord
                {
                    t = (int)Convert.ChangeType(i.Key, i.Key.GetTypeCode()),
                    u = i.Value ? 1 : 0,
                };

                outList.Add(data);
            }
            

            serializer.Serialize(writer, outList);
        }

        public override bool CanConvert(Type objectType)
        {
            var type = typeof(Dictionary<T, bool>);

            var fullName = objectType.ToString().Replace("&", "");

            var isMatch = fullName.Equals(type.ToString());
            
            return isMatch;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = serializer.Deserialize(reader, typeof(List<JsonRecord>));

            if (data == null) return default;

            var jsonRecords = (List<JsonRecord>) data;

            var outDict = new Dictionary<T, bool>();
            foreach (var record in jsonRecords)
            {
                outDict.Add((T)(object)record.t, record.u == 1);
            }

            return outDict;
        }
    }
}
