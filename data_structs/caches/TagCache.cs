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

    private TagData _RAM;
    private TagData _ROM;

    public override bool LoadData()
    {
        _ROM = new();

        // TODO: Read file


        _RAM = new(_ROM);
        return false;
    }

    public override void WriteData()
    {
        // Check if dirty, no point to write if not
        if (!_isDirty) return;

        ForceWrite();
    }

    public override void ForceWrite() { }

    public TagData.CommandParts GetExecutableCommand(string softwareTag, string projectName) => _RAM.GetCommandString(softwareTag, projectName);

    public string GetRAWCommand(string softwareTag) => _RAM.GetRawCommand(softwareTag);

    public bool HasSoftwareTag(string tag) => _RAM.HasSoftwareTag(tag);
}