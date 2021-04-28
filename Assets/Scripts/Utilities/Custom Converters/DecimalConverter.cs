using System;
using Newtonsoft.Json;

namespace  StarSalvager.Utilities.JSON.Converters
{
    public class DecimalConverter : JsonConverter<float>
    {
        public override void WriteJson(JsonWriter writer, float value, JsonSerializer serializer)
        {
            var simple = System.Math.Round(value, 2);

            serializer.Serialize(writer, simple);
        }

        public override float ReadJson(JsonReader reader, Type objectType, float existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var data = serializer.Deserialize(reader, typeof(float));

            if (data == null) return default;

            var value = (float) data;

            return value;
        }
    }
}
