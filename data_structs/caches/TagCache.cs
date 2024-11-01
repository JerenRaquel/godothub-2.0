public partial class TagCache : Cache
{
    #region Singleton Instance
    private static TagCache _instance;

    public static TagCache Instance => _instance;

    private TagCache(string userDirectory) : base(userDirectory, "/TagCache.gdhub") => LoadData();

    public static TagCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new TagCache(userDirectory);
            return _instance;
        }
    }
    #endregion

    private readonly TagData _RAM;
    private readonly TagData _ROM;

    public override bool LoadData() { return false; }

    public override void WriteData()
    {
        // Check if dirty, no point to write if not
        if (!_isDirty) return;

        ForceWrite();
    }

    public override void ForceWrite() { }

    public string GetExecutableCommand(string name) => _RAM.GetPath(name, null);
}