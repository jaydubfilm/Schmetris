using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarSalvager.Utilities.JsonDataTypes;

namespace StarSalvager.Utilities.Converters
{
    public class IBlockDataArrayConverter : JsonConverter<IEnumerable<IBlockData>>
    {
        public override void WriteJson(JsonWriter writer, IEnumerable<IBlockData> value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);
            JArray o = (JArray) t;
            o.WriteTo(writer);
        }

        public override IEnumerable<IBlockData> ReadJson(JsonReader reader, Type objectType,
            IEnumerable<IBlockData> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            var jArray = JArray.Load(reader);
            if (!jArray.HasValues)
            {
                if (objectType == typeof(IBlockData[]))
                {
                    return new IBlockData[0];
                }

                if (objectType == typeof(List<IBlockData>) || objectType == typeof(IEnumerable<IBlockData>))
                {
                    return new List<IBlockData>();
                }

                throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
            }
            var outData = new List<IBlockData>();
            foreach (var jObject in jArray)
            {
                var classType = (string)jObject[nameof(IBlockData.ClassType)];
                IBlockData iBlockData;
                switch (classType)
                {
                    case nameof(ScrapyardBit):
                    case nameof(Bit):
                        iBlockData = jObject.ToObject<BitData>();
                        break;
                    case nameof(ScrapyardPart):
                    case nameof(Part):
                        iBlockData = jObject.ToObject<PartData>();
                        break;
                    case nameof(JunkBit):
                        iBlockData = jObject.ToObject<JunkBitData>();
                        break;
                    case nameof(Crate):
                        iBlockData = jObject.ToObject<CrateData>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(classType), classType, null);
                }
                outData.Add(iBlockData);
            }
            if (objectType == typeof(IBlockData[]))
            {
                return outData.ToArray();
            }

            if (objectType == typeof(List<IBlockData>))
            {
                return outData.ToList();
            }

            if (objectType == typeof(IEnumerable<IBlockData>))
                return outData;
            throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
        }
    }
}
