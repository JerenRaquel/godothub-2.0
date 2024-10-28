using System.Collections.Generic;

public partial class VersionData
{
    private Dictionary<string, VersionObject> _data = [];

    public int Count => _data.Count;
    public string[] Keys => [.. _data.Keys];

    public bool AddVersion(ref VersionObject versionObj)
    {
        if (_data.ContainsKey(versionObj.Key)) return false;

        _data.Add(versionObj.Key, versionObj);
        return true;
    }

    public bool UpdateVersion(string oldKey, ref VersionObject versionObj)
    {
        if (!_data.ContainsKey(oldKey)) return false;
        if (!AddVersion(ref versionObj)) return false;

        _data.Remove(oldKey);
        return true;
    }

    public bool RemoveVersion(string key)
    {
        if (!_data.ContainsKey(key)) return false;

        _data.Remove(key);
        return true;
    }

    public string GetPath(string key)
    {
        if (!_data.TryGetValue(key, out VersionObject result)) return "";

        return result.Path;
    }

    public ParsedVersionKey ParseKey(string key)
    {
        if (!_data.TryGetValue(key, out VersionObject value)) return new ParsedVersionKey();

        return new ParsedVersionKey(value);
    }
}