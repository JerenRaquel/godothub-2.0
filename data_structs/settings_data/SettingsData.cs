using System.Collections.Generic;

public partial class SettingsData
{
    private Dictionary<string, Data> _data = [];

    public void AddOrUpdate(string tag, Data data)
    {
        if (!_data.TryAdd(tag, data)) _data[tag] = data;
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
}
