using System.IO;

public partial class VersionCache : Cache
{
    private VersionData _RAM;
    private VersionData _ROM;

    #region Singleton Instance
    private static VersionCache _instance;

    public static VersionCache Instance => _instance;

    private VersionCache(string userDirectory) : base(userDirectory + "/VersionCache.gdhub")
        => LoadData();

    public static VersionCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new VersionCache(userDirectory);
            return _instance;
        }
    }
    #endregion

    public string[] Keys => _RAM.Keys;

    public override bool LoadData()
    {
        _ROM = new();

        using (StreamReader file = new(SAVE_LOCATION))
        {
            string keyPaths = file.ReadLine();
            _ROM.LoadFullKeys(keyPaths);
            string builds = file.ReadLine();
            _ROM.LoadPartialKeys(builds);
        }

        _RAM = new(_ROM);
        return false;
    }

    public override void WriteData()
    {
        if (!_isDirty) return;

        ForceWrite();
    }

    public override void ForceWrite()
    {
        _ROM.OverwriteWith(_RAM);

        using (StreamWriter file = new(SAVE_LOCATION))
        {
            string keyPaths = _ROM.StringifyFullKeys();
            file.WriteLine(keyPaths);
            string builds = _ROM.StringifyPartialKeys();
            file.WriteLine(builds);
        }
    }

    public string AddVersion(Version version, bool isCSharp,
        VersionData.BuildType type, string path)
    {
        string key = _RAM.AddVersion(version, isCSharp, type, path);
        if (key.Length > 0) _isDirty = true;
        return key;
    }

    public bool UpdateVersion(string key, string path)
    {
        bool flag = _RAM.UpdateVersion(key, path);
        if (flag) _isDirty = true;
        return flag;
    }

    public string ReplaceKey(string oldKey, Version version, bool isCSharp,
        VersionData.BuildType type)
    {
        string newKey = _RAM.ReplaceKey(oldKey, version, isCSharp, type);
        if (newKey.Length > 0) _isDirty = true;
        return newKey;
    }

    public bool RemoveVersion(string key)
    {
        bool flag = _RAM.RemoveVersion(key);
        if (flag) _isDirty = true;
        return flag;
    }

    public string GetPath(string key) => _RAM.GetPath(key);

    public VersionData.BuildType[] GetAvailableBuilds(string partialKey)
        => _RAM.GetAvailableBuilds(partialKey);

    public bool HasKey(string key) => _RAM.HasKey(key);

    public bool HasPath(string path) => _RAM.HasPath(path);
}