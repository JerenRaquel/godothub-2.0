using System.Collections.Generic;
using Newtonsoft.Json;

public abstract class Cache(string saveLocation)
{
    protected readonly string SAVE_LOCATION = saveLocation;
    protected static readonly object padlock = new();

    protected bool _isDirty = false;

    public abstract bool LoadData();
    public abstract void WriteData();
    public abstract void ForceWrite();

    #region JsonText Helper Functions
    protected static void WriteEntry<T>(JsonTextWriter writer, string propName, T value)
    {
        // Prop : value
        writer.WritePropertyName(propName);
        writer.WriteValue(value);
    }

    protected static void WriterEntries<T>(JsonTextWriter writer, string propName, List<T> values)
    {
        writer.WritePropertyName(propName);
        // [
        writer.WriteStartArray();
        foreach (T value in values)
        {
            writer.WriteValue(value);
        }
        // ]
        writer.WriteEndArray();
    }

    protected static T ReadEntry<T>(JsonTextReader reader, T defaultValue)
    {
        if (!reader.Read()) return defaultValue;
        if (reader.TokenType == JsonToken.PropertyName) reader.Read();

        return reader.TokenType switch
        {
            JsonToken.StartObject => defaultValue,
            JsonToken.StartArray => defaultValue,
            _ => (T)reader.Value,
        };
    }

    protected static List<T> ReadEntries<T>(JsonTextReader reader)
    {
        List<T> data = [];

        reader.Read();
        if (reader.TokenType == JsonToken.PropertyName) reader.Read();
        if (reader.TokenType != JsonToken.StartArray) return data;

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                case JsonToken.PropertyName:
                    continue;
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    return data;
                default:
                    data.Add((T)reader.Value);
                    break;
            }
        }
        return data;
    }

    #endregion
}