using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarSalvager.PersistentUpgrades.Data;

namespace StarSalvager.Utilities.JSON.Converters
{
    internal struct UpgradeDataJson
    {
        public int type;
        public int bit;
        public int lvl;
    }
    
    public class UpgradeDataConverter : JsonConverter<UpgradeData>
    {
        public override void WriteJson(JsonWriter writer, UpgradeData value, JsonSerializer serializer)
        {
            var data = new UpgradeDataJson
            {
                type = (int) value.Type,
                bit = (int) value.BitType,
                lvl = value.Level
            };

            serializer.Serialize(writer, data);
        }

        public override UpgradeData ReadJson(JsonReader reader, Type objectType, UpgradeData existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var data = serializer.Deserialize(reader, typeof(UpgradeDataJson));

            if (data == null)
                return default;

            var UpgradeDataJson = (UpgradeDataJson) data;

            return new UpgradeData((UPGRADE_TYPE) UpgradeDataJson.type, (BIT_TYPE) UpgradeDataJson.bit,
                UpgradeDataJson.lvl);
        }
    }

    public class IEnumberableUpgradeDataConverter : JsonConverter<IEnumerable<UpgradeData>>
    {
        public override void WriteJson(JsonWriter writer, IEnumerable<UpgradeData> value, JsonSerializer serializer)
        {
            var values = value.ToArray();
            var container = new UpgradeDataJson[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                container[i] = new UpgradeDataJson
                {
                    type = (int) values[i].Type,
                    bit = (int) values[i].BitType,
                    lvl = values[i].Level
                };
            }

            JToken t = JToken.FromObject(container);
            JArray o = (JArray) t;
            o.WriteTo(writer);
        }

        public override IEnumerable<UpgradeData> ReadJson(JsonReader reader, Type objectType,
            IEnumerable<UpgradeData> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jArray = JArray.Load(reader);

            if (!jArray.HasValues)
            {
                if (objectType == typeof(UpgradeData[]))
                {
                    return new UpgradeData[0];
                }

                if (objectType == typeof(List<UpgradeData>) || objectType == typeof(IEnumerable<UpgradeData>))
                {
                    return new List<UpgradeData>();
                }

                throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
            }

            var outData = new List<UpgradeData>();
            foreach (var jObject in jArray)
            {
                var data = jObject.ToObject<UpgradeDataJson>();

                outData.Add(new UpgradeData(
                    (UPGRADE_TYPE)data.type,
                    (BIT_TYPE)data.bit,
                    data.lvl));
            }

            if (objectType == typeof(UpgradeData[]))
            {
                return outData.ToArray();
            }

            if (objectType == typeof(List<UpgradeData>))
            {
                return outData.ToList();
            }

            if (objectType == typeof(IEnumerable<UpgradeData>))
                return outData;

            throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
        }
    }
}
