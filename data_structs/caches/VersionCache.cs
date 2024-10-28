public partial class VersionCache : Cache
{
    private VersionData _RAM;
    private VersionData _ROM;

    public int Count => _RAM.Count;
    public string[] Keys => _RAM.Keys;

    #region Singleton Instance
    private static VersionCache _instance;

    public static VersionCache Instance => _instance;

    private VersionCache(string userDirectory) : base(userDirectory + "/VersionCache.gdhub")
    {
        LoadData();
    }

    public static VersionCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new VersionCache(userDirectory);
            return _instance;
        }
    }
    #endregion

    public override bool LoadData()
    {
        // TODO: Read from file

        return false;
    }

    public override void WriteData()
    {
        if (!_isDirty) return;

        ForceWrite();
    }

    public override void ForceWrite()
    {
        // TODO: Finish
    }

    public string AddVersion(Version version, bool isCSharp, VersionData.BuildType type, string path)
    {
        VersionData.VersionObject obj = new(version, isCSharp, type, path);
        if (!_RAM.AddVersion(ref obj)) return "";
        return obj.Key;
    }

    public string GetPath(string key) => _RAM.GetPath(key);

    public VersionData.ParsedVersionKey GetVersionData(string key) => _RAM.ParseKey(key);
}