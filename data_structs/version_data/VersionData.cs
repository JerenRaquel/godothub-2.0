using System;
using System.Collections.Generic;

public partial class VersionData
{
    private Dictionary<string, string> _keyToPath = [];
    private Dictionary<string, HashSet<BuildType>> _partialKeyToBuilds = [];
    private Dictionary<string, List<string>> _versions = [];

    public string[] Keys => [.. _keyToPath.Keys];
    public string[] SortedKeys
    {
        get
        {
            string[] data = [.. _keyToPath.Keys];
            Array.Sort(data);
            return data;
        }
    }
    public string[] Versions => [.. _versions.Keys];

    public VersionData() { }

    public VersionData(VersionData other) => OverwriteWith(other);

    public string AddVersion(Version version, bool isCSharp, BuildType type, string path)
    {
        // Item1: Full Key | Item2: Partial Key
        Tuple<string, string> keys = GenerateKeys(version, isCSharp, type);

        // Check if we have the full key -- the partial will exist if full does
        if (_keyToPath.ContainsKey(keys.Item1)) return "";

        if (!_partialKeyToBuilds.ContainsKey(keys.Item2))
            _partialKeyToBuilds.Add(keys.Item2, []);

        if (!_versions.ContainsKey(version.ToString()))
            _versions.Add(version.ToString(), []);

        _keyToPath.Add(keys.Item1, path);
        _partialKeyToBuilds[keys.Item2].Add(type);
        _versions[version.ToString()].Add(keys.Item1);
        return keys.Item1;
    }

    public bool UpdateVersion(string key, string path)
    {
        if (!_keyToPath.ContainsKey(key)) return false;

        _keyToPath[key] = path;
        return true;
    }

    public string ReplaceKey(string oldKey, Version version, bool isCSharp, BuildType type)
    {
        string newKey = GenerateKey(version, isCSharp, type);
        // Check if we have the full key -- the partial will exist if full does
        if (_keyToPath.ContainsKey(newKey)) return oldKey;

        // Fetch and check if old key works
        string path = GetPath(oldKey);
        if (path.Length == 0) return oldKey;
        if (!RemoveVersion(oldKey)) return oldKey;

        AddVersion(version, isCSharp, type, path);
        return newKey;
    }

    public bool RemoveVersion(string key)
    {
        // Check if full key exists
        if (!_keyToPath.ContainsKey(key)) return false;

        // Check if key can be unpacked
        ParsedVersionKey parts = ParseKey(key);
        if (!parts.isValid) return false;

        // Check if partial exists
        string partialKey = GeneratePartialKey(parts.version, parts.isCSharp);
        if (!_partialKeyToBuilds.ContainsKey(partialKey)) return false;
        if (!_partialKeyToBuilds[partialKey].Contains(parts.build)) return false;

        // Success
        _keyToPath.Remove(key);
        _partialKeyToBuilds[partialKey].Remove(parts.build);
        if (_partialKeyToBuilds[partialKey].Count == 0) _partialKeyToBuilds.Remove(partialKey);
        _versions[parts.version.ToString()].Remove(key);
        if (_versions[parts.version.ToString()].Count == 0) _versions.Remove(parts.version.ToString());
        return true;
    }

    public string GetPath(string key)
    {
        if (!_keyToPath.TryGetValue(key, out string value)) return "";
        return value;
    }

    public BuildType[] GetAvailableBuilds(string partialKey)
    {
        if (!_partialKeyToBuilds.TryGetValue(partialKey, out HashSet<BuildType> value)) return [];
        return [.. value];
    }

    public bool HasKey(string key) => _keyToPath.ContainsKey(key);

    public bool HasPartialKey(string partialKey) => _partialKeyToBuilds.ContainsKey(partialKey);

    public bool HasPath(string path) => _keyToPath.ContainsValue(path);

    public void OverwriteWith(VersionData other)
    {
        if (Equals(other)) return;

        _keyToPath = [];
        _partialKeyToBuilds = [];
        _versions = [];

        foreach (KeyValuePair<string, string> entry in other._keyToPath)
            _keyToPath.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, HashSet<BuildType>> entry in other._partialKeyToBuilds)
        {
            _partialKeyToBuilds.Add(entry.Key, []);
            foreach (BuildType type in entry.Value)
                _partialKeyToBuilds[entry.Key].Add(type);
        }

        foreach (KeyValuePair<string, List<string>> entry in other._versions)
        {
            _versions.Add(entry.Key, []);
            foreach (string item in entry.Value)
                _versions[entry.Key].Add(item);
        }
    }

}