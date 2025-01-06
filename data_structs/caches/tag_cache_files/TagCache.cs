
public partial class TagCache : Cache
{
    public void AddOrUpdateSoftwareTag(string name, TagData.SoftwareData data) => _RAM.AddOrUpdateSoftwareTag(name, data);

    public void AddOrUpdateProjectTag(string name, string colorCode) => _RAM.AddOrUpdateProjectTagColor(name, colorCode);

    public TagData.CommandParts GetExecutableCommand(string softwareTag, string projectName) => _RAM.GetCommandString(softwareTag, projectName);

    public string GetRAWCommand(string softwareTag, bool full = true) => _RAM.GetRawCommand(softwareTag, full);

    public string GetPath(string softwareTag) => _RAM.GetPath(softwareTag);

    public string GetArgString(string softwareTag) => _RAM.GetArgString(softwareTag);

    public string GetColor(bool isSoftware, string tag, string defaultValue = "FFFFFF") => _RAM.GetColor(isSoftware, tag, defaultValue);

    public bool IsFavorited(string softwareTag) => _RAM.IsFavorited(softwareTag);

    public bool HasSoftwareTag(string softwareTag) => _RAM.HasSoftwareTag(softwareTag);

    public bool HasProjectTag(string projectTag) => _RAM.HasProjectTag(projectTag);

    public void SetFavorited(string softwareTag, bool state) => _RAM.UpdateFavoriteState(softwareTag, state);
}