using System;
using System.Collections.Generic;

public partial class SettingsData
{
    private Dictionary<string, Data> _data = [];

    public int Count => _data.Count;
    public Dictionary<string, Data>.Enumerator RawData => _data.GetEnumerator();
    public string[] Keys => [.. _data.Keys];

    public SettingsData() { }
    public SettingsData(SettingsData other) => OverwriteWith(other);

    public void AddOrUpdate(string tag, Data data)
    {
        if (!_data.TryAdd(tag, data)) _data[tag] = data;
    }

    public void AddEntryToDataList(string tag, string entry)
    {
        if (!_data.ContainsKey(tag)) return;
        if (!_data[tag].IsArray) return;


        string[] data = GetData(tag);
        string[] newData = new string[data.Length + 1];
        int i = 0;
        foreach (string str in data)
        {
            newData[i] = str;
            i++;
        }
        newData[i] = entry;

        _data[tag] = newData;
    }

    public void RemoveEntryFromStringList(string tag, string entry)
    {
        if (!_data.ContainsKey(tag)) return;
        if (!_data[tag].IsArray) return;

        string[] data = GetData(tag);
        string[] newData = new string[data.Length - 1];
        int i = 0;
        foreach (string str in data)
        {
            if (str != entry)
            {
                newData[i] = str;
                i++;
            }
            else continue;
        }

        _data[tag] = newData;
    }

    public bool Erase(string tag)
    {
        if (!_data.ContainsKey(tag)) return false;

        _data.Remove(tag);
        return true;
    }

    public Data GetData(string tag)
    {
        if (!_data.TryGetValue(tag, out Data value)) return new Data();
        return value;
    }

    public void OverwriteWith(SettingsData other)
    {
        foreach (KeyValuePair<string, Data> entry in other._data)
            AddOrUpdate(entry.Key, entry.Value);
    }

    public void LoadData(string key, string dataStr)
    {
        if (key.EndsWith(TypeToString(Type.NULL))) return;

        if (key.EndsWith(TypeToString(Type.STRING_LIST)))
        {
            string santized = dataStr.Replace("[", "").Replace("]", "");
            string[] parts = santized.Split(",", StringSplitOptions.RemoveEmptyEntries);
            AddOrUpdate(key, parts);
            return;
        }

        if (key.EndsWith(TypeToString(Type.BOOL)))
        {
            AddOrUpdate(key, new(dataStr == "TRUE"));
            return;
        }

        if (!Int64.TryParse(dataStr, out long result)) return;
        AddOrUpdate(key, new(result));
    }

    public static string GenerateKey(string group, string section, string tag, Type type)
        => $"{group}/{section}/{tag}/{type}";

    public static ParsedKeyData ParseKey(string key)
    {
        string[] parts = key.Split("/", StringSplitOptions.RemoveEmptyEntries);
        return new ParsedKeyData(parts[0], parts[1], parts[2]);
    }
}
