using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public abstract class Cache(string userDirectory, string saveFolder)
{
    protected readonly string SAVE_LOCATION = userDirectory + saveFolder;
    protected readonly string USER_DIRECTORY = userDirectory;
    protected static readonly object padlock = new();

    protected bool _isDirty = false;

    public abstract bool LoadData();
    public abstract void WriteData();
    public abstract void ForceWrite();

    #region JsonText Helper Functions
    public static void WriteEntry<T>(JsonTextWriter writer, string propName, T value)
    {
        // Prop : value
        writer.WritePropertyName(propName);
        writer.WriteValue(value);
    }

    public static void WriterEntries<T>(JsonTextWriter writer, string propName, List<T> values)
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

    public static T ReadEntry<T>(JsonTextReader reader, T defaultValue)
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

    public static Tuple<string, T> ReadEntryWithProp<T>(JsonTextReader reader, T defaultValue)
    {
        if (!reader.Read()) return null;
        if (reader.TokenType != JsonToken.PropertyName) return null;

        string propName = (string)reader.Value;

        reader.Read();
        T value = reader.TokenType switch
        {
            JsonToken.StartObject => defaultValue,
            JsonToken.StartArray => defaultValue,
            _ => (T)reader.Value,
        };

        return new Tuple<string, T>(propName, value);
    }

    public static List<T> ReadEntries<T>(JsonTextReader reader)
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

    public static Tuple<string, List<T>> ReadEntriesWithProp<T>(JsonTextReader reader)
    {
        List<T> data = [];
        string propName = "";

        reader.Read();
        if (reader.TokenType == JsonToken.PropertyName) propName = (string)reader.Value;

        reader.Read();
        if (reader.TokenType != JsonToken.StartArray) return null;

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
                    return new Tuple<string, List<T>>(propName, data);
                default:
                    data.Add((T)reader.Value);
                    break;
            }
        }
        return new Tuple<string, List<T>>(propName, data);
    }

    #endregion
}