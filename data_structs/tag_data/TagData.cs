using System.Collections.Generic;

public partial class TagData
{
    private Dictionary<string, SoftwareData> _softwareTags = [];
    // { tag : colorCode }
    private Dictionary<string, string> _projectTags = [];

    public void AddOrUpdateSoftwareTag(string name, SoftwareData data)
    {
        if (_softwareTags.ContainsKey(name))
        {
            _softwareTags.Add(name, data);
            return;
        }
        _softwareTags[name] = data;
    }

    public void AddOrUpdateProjectTagColor(string name, string colorCode)
    {
        if (_projectTags.ContainsKey(name))
        {
            _projectTags.Add(name, colorCode);
            return;
        }
        _projectTags[name] = colorCode;
    }

    public string GetColor(bool isSoftware, string name, string defaultValue = "000000")
    {
        if (isSoftware)
        {
            if (!_softwareTags.TryGetValue(name, out SoftwareData data)) return defaultValue;
            if (data.IsNull) return defaultValue;
            return data.ColorCode;
        }
        else
        {
            if (!_projectTags.TryGetValue(name, out string colorCode)) return defaultValue;
            return colorCode;
        }
    }

    public string GetRawCommand(string softwareTag)
    {
        if (!_softwareTags.TryGetValue(softwareTag, out SoftwareData data)) return null;
        if (data.IsNull) return null;
        return data.RAWCommand;
    }

    public CommandParts GetCommandString(string softwareTag, string projectName)
    {
        if (!_softwareTags.TryGetValue(softwareTag, out SoftwareData data)) return new();
        if (data.IsNull) return new();

        return SetMacros(data.CommandData, projectName);
    }

    public void Overwrite(TagData other)
    {
        _softwareTags = [];
        _projectTags = [];

        foreach (KeyValuePair<string, SoftwareData> entry in other._softwareTags)
            _softwareTags.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, string> entry in other._projectTags)
            _projectTags.Add(entry.Key, entry.Value);
    }
}