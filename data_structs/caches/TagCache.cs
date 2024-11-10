using System.Collections.Generic;
using System.IO;

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

    public string[] SoftwareTags => _RAM.SoftwareTabs;
    public string[] FavoritedSoftwareTags => _RAM.GetAllFavoritedSoftwareTags();
    public string[] ProjectTags => _RAM.ProjectTabs;

    public override bool LoadData()
    {
        _ROM = new();

        bool fileRead = false;
        if (File.Exists(SAVE_LOCATION))
        {
            if (new FileInfo(SAVE_LOCATION).Length > 0)
            {
                using (StreamReader file = new(SAVE_LOCATION))
                {
                    string key = file.ReadLine();
                    string data = file.ReadLine();

                    while (key != null && data != null)
                    {
                        _ROM.LoadData(key, data);
                        key = file.ReadLine();
                        data = file.ReadLine();
                    }
                    fileRead = true;
                }
            }
        }

        _RAM = new(_ROM);
        return fileRead;
    }

    public override void WriteData()
    {
        // Check if dirty, no point to write if not
        if (!_isDirty) return;

        ForceWrite();
    }

    public override void ForceWrite()
    {

        _ROM.OverwriteWith(_RAM);

        using StreamWriter file = new(SAVE_LOCATION, false);

        // No Data to write -- Wipe file
        if (_ROM.ProjectCount == 0 && _ROM.SoftwareCount == 0)
        {
            file.Close();
            return;
        }

        using (Dictionary<string, TagData.SoftwareData>.Enumerator romEnumerator = _ROM.RawSoftwareData)
        {
            while (romEnumerator.MoveNext())
            {
                KeyValuePair<string, TagData.SoftwareData> entry = romEnumerator.Current;
                if (entry.Value.IsNull) continue;

                file.WriteLine(entry.Key);
                file.WriteLine(entry.Value.ToJSONString());
            }
        }
        using (Dictionary<string, string>.Enumerator romEnumerator = _ROM.RawProjectData)
        {
            while (romEnumerator.MoveNext())
            {
                KeyValuePair<string, string> entry = romEnumerator.Current;
                if (entry.Value == null || entry.Value.Length == 0) continue;

                file.WriteLine(entry.Key);
                file.WriteLine(entry.Value);
            }
        }

        file.Close();
    }

    public void AddOrUpdateSoftwareTag(string name, TagData.SoftwareData data) => _RAM.AddOrUpdateSoftwareTag(name, data);

    public TagData.CommandParts GetExecutableCommand(string softwareTag, string projectName) => _RAM.GetCommandString(softwareTag, projectName);

    public string GetRAWCommand(string softwareTag, bool full = true) => _RAM.GetRawCommand(softwareTag, full);

    public string GetPath(string softwareTag) => _RAM.GetPath(softwareTag);

    public string GetArgString(string softwareTag) => _RAM.GetArgString(softwareTag);

    public string GetColor(bool isSoftware, string tag, string defaultValue = "FFFFFF") => _RAM.GetColor(isSoftware, tag, defaultValue);

    public bool IsFavorited(string softwareTag) => _RAM.IsFavorited(softwareTag);

    public bool HasSoftwareTag(string softwareTag) => _RAM.HasSoftwareTag(softwareTag);

    public void SetFavorited(string softwareTag, bool state) => _RAM.UpdateFavoriteState(softwareTag, state);
}