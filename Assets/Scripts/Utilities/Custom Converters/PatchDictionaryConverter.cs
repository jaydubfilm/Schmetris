using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StarSalvager.Utilities.JSON.Converters
{
    public class PatchDictionaryConverter : JsonConverter
    {
        private struct JsonRecord
        {
            public int t;
            public int l;
            public int u;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = (Dictionary<PatchData, bool>) value;
            var outList = new List<JsonRecord>();
            foreach (var i in val)
            {
                var data = new JsonRecord
                {
                    t = i.Key.Type,
                    l = i.Key.Level,
                    u = i.Value ? 1 : 0,
                };

                outList.Add(data);
            }
            

            serializer.Serialize(writer, outList);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = serializer.Deserialize(reader, typeof(List<JsonRecord>));

            if (data == null) return default;

            var jsonRecords = (List<JsonRecord>) data;

            var outDict = new Dictionary<PatchData, bool>();
            foreach (var record in jsonRecords)
            {
                outDict.Add(new PatchData
                {
                    Type = record.t,
                    Level = record.l
                }, record.u == 1);
            }

            return outDict;
        }

        public override bool CanConvert(Type objectType)
        {
            var type = typeof(Dictionary<PatchData, bool>);
            
            var fullName = objectType.ToString().Replace("&", "");

            var isMatch = fullName.Equals(type.ToString());
            
            return isMatch;
        }
    }
}
