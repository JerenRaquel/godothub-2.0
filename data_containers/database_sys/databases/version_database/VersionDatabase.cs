using System;
using System.Collections.Generic;

namespace DataContainer.DatabaseSys.Databases.VersionDatabase
{
    public partial class VersionDatabase : Database<VersionKey, string>
    {
        private Dictionary<string, HashSet<BuildType>> _partialKeyToBuilds = [];
        private Dictionary<Version, List<VersionKey>> _versions = [];

        public VersionKey[] Keys => [.. _data.Keys];
        public VersionKey[] SortedKeys
        {
            get
            {
                VersionKey[] data = Keys;
                Array.Sort(data, reverseComparer);
                return data;
            }
        }
        public Version[] Versions => [.. _versions.Keys];
        public int Count => _data.Count;

        public bool HasPartialKey(string partialKey)
            => _partialKeyToBuilds.ContainsKey(partialKey);

        public bool HasPath(string path) => _data.ContainsValue(path);

        public VersionKey AddVersion(Version version, bool isDotNet, BuildType type,
            string path)
        {
            VersionKey key = new(version, isDotNet, type);
            if (!AddVersion(key, path)) return null;
            return key;
        }

        public bool AddVersion(VersionKey key, string path)
        {
            //* Check if we have the full key
            //? Partial will always be valid
            if (_data.ContainsKey(key)) return false; // Duplicate Error

            //* Add the Partial key
            if (!_partialKeyToBuilds.ContainsKey(key.PartialKey))
                _partialKeyToBuilds.Add(key.PartialKey, []);

            if (!_versions.ContainsKey(key.version))
                _versions.Add(key.version, []);

            _data.Add(key, path);
            _partialKeyToBuilds[key.PartialKey].Add(key.buildType);
            _versions[key.version].Add(key);
            IsDirty = true;
            return true;
        }

        public bool UpdateVersion(VersionKey key, string path)
        {
            if (_data.ContainsKey(key)) return false;

            _data[key] = path;
            IsDirty = true;
            return true;
        }

        public VersionKey ReplaceKey(VersionKey oldKey, Version version, bool isDotNet,
            BuildType type)
        {
            VersionKey newKey = new(version, isDotNet, type);

            // Check if we have the full key -- partial will exists if full doesn't
            if (_data.ContainsKey(newKey)) return oldKey;

            // Fetch and check if old key works
            string path = GetPath(oldKey);
            if (path == null) return oldKey;
            if (!RemoveVersion(oldKey)) return oldKey;

            if (!AddVersion(newKey, path)) return oldKey;
            return newKey;
        }

        public bool RemoveVersion(VersionKey key)
        {
            // Check if key is valid
            if (!key.IsValid) return false;

            // Check if full key exists
            if (!_data.ContainsKey(key)) return false;

            // Check if partial exists
            if (!_partialKeyToBuilds.ContainsKey(key.PartialKey)) return false;
            if (!_partialKeyToBuilds[key.PartialKey].Contains(key.buildType)) return false;

            // Success == Remove it
            _data.Remove(key);
            _partialKeyToBuilds[key.PartialKey].Remove(key.buildType);
            if (_partialKeyToBuilds[key.PartialKey].Count == 0)
                _partialKeyToBuilds.Remove(key.PartialKey);
            _versions[key.version].Remove(key);
            if (_versions[key.version].Count == 0)
                _versions.Remove(key.version);
            IsDirty = true;
            return true;
        }

        public string GetPath(VersionKey key)
        {
            if (!_data.TryGetValue(key, out string path)) return null;
            return path;
        }

        public BuildType[] GetAvaliableBuilds(string partialKey)
        {
            if (!_partialKeyToBuilds.TryGetValue(partialKey, out HashSet<BuildType> types))
                return [];
            return [.. types];
        }
    }
}