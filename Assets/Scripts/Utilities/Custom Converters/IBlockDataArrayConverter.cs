using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarSalvager.Utilities.JsonDataTypes;

namespace StarSalvager.Utilities.Converters
{
    public class IBlockDataArrayConverter : JsonConverter<IBlockData[]>
    {
        public override void WriteJson(JsonWriter writer, IBlockData[] value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);
            JArray o = (JArray) t;
            o.WriteTo(writer);
        }

        public override IBlockData[] ReadJson(JsonReader reader, Type objectType, IBlockData[] existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jArray = JArray.Load(reader);

            if (!jArray.HasValues)
                return new IBlockData[0];

            var outData = new List<IBlockData>();
            foreach (var jObject in jArray)
            {
                var classType = (string) jObject[nameof(IBlockData.ClassType)];

                IBlockData iBlockData = classType switch
                {
                    nameof(Bit) => jObject.ToObject<BitData>(),
                    nameof(Part) => jObject.ToObject<PartData>(),
                    _ => throw new ArgumentOutOfRangeException(nameof(classType), classType, null)
                };

                outData.Add(iBlockData);
            }

            return outData.ToArray();
        }
    }
}
