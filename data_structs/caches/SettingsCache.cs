public partial class SettingsCache : Cache
{
    private SettingsData _RAM;
    private SettingsData _ROM;

    #region Singleton Instance
    private static SettingsCache _instance;

    public static SettingsCache Instance => _instance;

    private SettingsCache(string userDirectory) : base(userDirectory + "/SettingsCache.gdhub")
    {
        LoadData();
    }

    public static SettingsCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new SettingsCache(userDirectory);
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
        _ROM.OverwriteWith(_RAM);

        // TODO: Write to file

        _isDirty = false;
    }

    public void AddOrUpdate(string tag, SettingsData.Data data) => _RAM.AddOrUpdate(tag, data);

    public bool Erase(string tag) => _RAM.Erase(tag);

    public SettingsData.Data GetData(string tag) => _RAM.GetData(tag);

}