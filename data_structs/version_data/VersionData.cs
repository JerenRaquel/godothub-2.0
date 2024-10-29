using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public partial class VersionData
{
    private Dictionary<string, string> _keyToPath = [];
    private Dictionary<string, HashSet<BuildType>> _partialKeyToBuilds = [];

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

        _keyToPath.Add(keys.Item1, path);
        _partialKeyToBuilds[keys.Item2].Add(type);
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

        foreach (KeyValuePair<string, string> entry in other._keyToPath)
            _keyToPath.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, HashSet<BuildType>> entry in other._partialKeyToBuilds)
        {
            _partialKeyToBuilds.Add(entry.Key, []);
            foreach (BuildType type in entry.Value)
                _partialKeyToBuilds[entry.Key].Add(type);
        }
    }

}