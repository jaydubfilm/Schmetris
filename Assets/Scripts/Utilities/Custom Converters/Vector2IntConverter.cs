using System;
using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

public class Vector2IntConverter : JsonConverter<Vector2Int>
{
    
    
    public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
    {
        var data = new jsonV2Int
        {
            x = value.x,
            y = value.y
        };
        
        serializer.Serialize(writer, data);
    }

    public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var data = serializer.Deserialize(reader, typeof(jsonV2Int));

        if (data == null)
            return Vector2Int.zero;

        var coord = (jsonV2Int) data;
        
        return new Vector2Int(coord.x, coord.y);
    }
}
