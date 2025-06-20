using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DataContainer.DatabaseSys
{
    // K for key
    // C for container type; ie. project, settings, tags, etc.
    public abstract partial class Database<K, C>(string userDirectory, string saveFolder)
    {
        public enum ImportError { OK, READ_FAIL, INVALID_CONFIG_VERSION }

        protected readonly struct FileData(ImportError error, StreamReader file)
        {
            public readonly ImportError error = error;
            public readonly StreamReader file = file;
        }

        protected readonly string SAVE_LOCATION = userDirectory + saveFolder;
        protected readonly string USER_DIRECTORY = userDirectory;
        protected static readonly object padlock = new();

        protected readonly int READABLE_VERSION = 1;
        protected readonly Dictionary<K, C> _data = [];

        public bool IsDirty { get; protected set; } = false;

        public abstract bool LoadData();
        public abstract void ForceWrite();

        public void WriteData()
        {
            if (!IsDirty) return;
            ForceWrite();
        }

        public virtual bool HasKey(K key) => _data.ContainsKey(key);

        // TODO: Replace with connection to Logger
        protected bool LogReadableFileAccess(in FileData data)
        {
            if (data.file == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to load data from {SAVE_LOCATION}.");
                Console.WriteLine($"Error: {data.error}.");
                Console.ResetColor();
                return false;
            }
            return true;
        }

        protected FileData OpenReadableFile()
        {
            if (!File.Exists(SAVE_LOCATION)) return new(ImportError.READ_FAIL, null);

            StreamReader file = new(SAVE_LOCATION);
            string versionFlag = file.ReadLine();
            versionFlag = versionFlag.Replace("Version: ", "");
            if (!ushort.TryParse(versionFlag, out ushort versionNumber))
            {
                file.Close();
                return new(ImportError.READ_FAIL, null);
            }
            if (READABLE_VERSION == versionNumber)
                return new(ImportError.OK, file);

            return new(ImportError.INVALID_CONFIG_VERSION, null);
        }

        protected StreamWriter OpenWritableFile()
        {
            StreamWriter file = new(SAVE_LOCATION, false);
            file.WriteLine($"Version: {READABLE_VERSION}");
            return file;
        }

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
}