using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarSalvager;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.JSON.Converters
{
    public class IEnumberableVector2IntConverter : JsonConverter<IEnumerable<Vector2Int>>
    {
        public override void WriteJson(JsonWriter writer, IEnumerable<Vector2Int> value, JsonSerializer serializer)
        {
            var values = value.ToArray();
            var container = new jsonV2Int[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                container[i] = new jsonV2Int
                {
                    x = values[i].x,
                    y = values[i].y
                };
            }

            JToken t = JToken.FromObject(container);
            JArray o = (JArray) t;
            o.WriteTo(writer);
        }

        public override IEnumerable<Vector2Int> ReadJson(JsonReader reader, Type objectType,
            IEnumerable<Vector2Int> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jArray = JArray.Load(reader);

            if (!jArray.HasValues)
            {
                if (objectType == typeof(Vector2Int[]))
                {
                    return new Vector2Int[0];
                }

                if (objectType == typeof(List<Vector2Int>) || objectType == typeof(IEnumerable<Vector2Int>))
                {
                    return new List<Vector2Int>();
                }

                throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
            }

            var outData = new List<Vector2Int>();
            foreach (var jObject in jArray)
            {
                var coord = jObject.ToObject<jsonV2Int>();

                outData.Add(new Vector2Int
                {
                    x = coord.x,
                    y = coord.y
                });
            }

            if (objectType == typeof(Vector2Int[]))
            {
                return outData.ToArray();
            }

            if (objectType == typeof(List<Vector2Int>))
            {
                return outData.ToList();
            }

            if (objectType == typeof(IEnumerable<Vector2Int>))
                return outData;

            throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
        }
    }
}
